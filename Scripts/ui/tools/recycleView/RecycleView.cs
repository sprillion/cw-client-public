using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class Adapter : MonoBehaviour
{
    public abstract event Action OnDataChange;
    public abstract int GetItemCount();
    public abstract GameObject CreateView(int index, Transform parent);
    public abstract void BindView(GameObject view, int index);
}

[RequireComponent(typeof(ScrollRect))]
public class RecycleView : MonoBehaviour
{
    [SerializeField] private Adapter _adapter;
    [SerializeField] private float _spacing = 0f;

    private ScrollRect _scrollRect;
    private RectTransform _content;
    
    private readonly List<GameObject> _activeViews = new List<GameObject>();
    private readonly Queue<GameObject> _viewPool = new Queue<GameObject>();
    
    private float itemHeight;
    private int totalItems;
    private int currentStartIndex = -1;
    private int currentEndIndex = -1;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _content = _scrollRect.content;
        _scrollRect.onValueChanged.AddListener(OnScroll);
        _adapter.OnDataChange += NotifyDataSetChanged;
        NotifyDataSetChanged();
    }

    private void Initialize()
    {
        if (_adapter == null || _adapter.GetItemCount() == 0)
        {
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, 0);
            return;
        }

        // Get first item height
        SetItemHeight();

        totalItems = _adapter.GetItemCount();
        float contentHeight = totalItems * (itemHeight + _spacing) - _spacing;
        _content.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight);

        Vector2 viewportSize = _scrollRect.viewport.rect.size;
        int visibleItems = Mathf.CeilToInt(viewportSize.y / (itemHeight + _spacing)) + 1;

        currentStartIndex = 0;
        currentEndIndex = Mathf.Min(visibleItems, totalItems - 1);
        
        for (int i = 0; i <= currentEndIndex; i++)
        {
            CreateItem(i);
        }
    }

    private void OnScroll(Vector2 pos)
    {
        if (_activeViews.Count == 0) return;

        int newStartIndex = GetStartIndex();
        int newEndIndex = GetEndIndex(newStartIndex);

        // Добавляем ограничения для индексов
        newStartIndex = Mathf.Clamp(newStartIndex, 0, totalItems - 1);
        newEndIndex = Mathf.Clamp(newEndIndex, 0, totalItems - 1);

        if (newStartIndex > currentEndIndex || newEndIndex < currentStartIndex)
        {
            // Полная перерисовка если вышли за пределы
            RecreateAllViews(newStartIndex, newEndIndex);
            return;
        }

        // Обработка прокрутки ВВЕРХ (добавляем элементы в начало)
        while (currentStartIndex > newStartIndex)
        {
            currentStartIndex--;
            CreateItem(currentStartIndex, true); // Добавляем в начало
        }

        // Обработка прокрутки ВНИЗ (добавляем элементы в конец)
        while (currentEndIndex < newEndIndex)
        {
            currentEndIndex++;
            CreateItem(currentEndIndex);
        }

        // Удаляем элементы сверху
        while (currentStartIndex < newStartIndex)
        {
            ReturnViewToPool(_activeViews[0]);
            _activeViews.RemoveAt(0);
            currentStartIndex++;
        }

        // Удаляем элементы снизу
        while (currentEndIndex > newEndIndex)
        {
            ReturnViewToPool(_activeViews[^1]);
            _activeViews.RemoveAt(_activeViews.Count - 1);
            currentEndIndex--;
        }
    }
    
    private void RecreateAllViews(int newStart, int newEnd)
    {
        foreach (var view in _activeViews) ReturnViewToPool(view);
        _activeViews.Clear();
    
        currentStartIndex = newStart;
        currentEndIndex = newEnd;
    
        for (int i = currentStartIndex; i <= currentEndIndex; i++)
        {
            CreateItem(i);
        }
    }
    private int GetStartIndex()
    {
        float scrollPos = _content.anchoredPosition.y;
        return Mathf.FloorToInt(scrollPos / (itemHeight + _spacing));
    }

    private int GetEndIndex(int startIndex)
    {
        Vector2 viewportSize = _scrollRect.viewport.rect.size;
        int visibleItems = Mathf.CeilToInt(viewportSize.y / (itemHeight + _spacing));
        return Mathf.Min(startIndex + visibleItems, totalItems - 1);
    }

    private void CreateItem(int index, bool atStart = false)
    {
        GameObject view = GetViewFromPool();
        RectTransform rect = view.GetComponent<RectTransform>();

        var posIndex = index - totalItems / 2f + 1;
        float yPos = -posIndex * (itemHeight + _spacing);
        rect.anchoredPosition = new Vector2(0, yPos);
    
        _adapter.BindView(view, index);
    
        if (atStart) 
            _activeViews.Insert(0, view);
        else 
            _activeViews.Add(view);
    }

    private GameObject GetViewFromPool()
    {
        if (_viewPool.Count > 0)
        {
            GameObject view = _viewPool.Dequeue();
            view.SetActive(true);
            return view;
        }
        return _adapter.CreateView(currentStartIndex + _activeViews.Count, _content);
    }

    private void ReturnViewToPool(GameObject view)
    {
        view.SetActive(false);
        _viewPool.Enqueue(view);
    }

    private void NotifyDataSetChanged()
    {
        foreach (var view in _activeViews)
        {
            ReturnViewToPool(view);
        }
        _activeViews.Clear();
        Initialize();
    }

    private void SetItemHeight()
    {
        if (itemHeight > 0) return;
        
        GameObject sampleItem = _adapter.CreateView(0, _content);
        itemHeight = sampleItem.GetComponent<RectTransform>().rect.height;
        ReturnViewToPool(sampleItem);
    }
}