using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Zero.Core.Collections
{
    /// <summary>
    /// 扩展的 ObservableCollection，支持批量添加元素并只触发一次集合变更通知。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZeroObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// 用于标记是否正在执行批量操作，防止在批量操作期间触发多次通知。
        /// </summary>
        private bool _suppressNotification;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ZeroObservableCollection() : base() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collection"></param>
        public ZeroObservableCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="list"></param>
        public ZeroObservableCollection(List<T> list) : base(list) { }

        /// <summary>
        /// 批量添加元素，并只触发一次 CollectionChanged 通知（Reset）。
        /// </summary>
        /// <param name="collection">要添加的元素集合。</param>
        /// <exception cref="ArgumentNullException">当 collection 为 null 时抛出。</exception>
        public void AddRange(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            // 挂起通知，避免在循环添加时频繁触发 UI 更新
            _suppressNotification = true;

            try
            {
                foreach (var item in collection)
                {
                    // 直接操作 Items 属性，绕过 OnCollectionChanged 的单次触发
                    Items.Add(item);
                }
            }
            finally
            {
                // 恢复通知
                _suppressNotification = false;
            }

            // 批量添加完成后，只触发一次 Reset 通知，让 UI 整体刷新
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }

        /// <summary>
        /// 重写 OnCollectionChanged，在批量操作期间抑制通知。
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        /// <summary>
        /// 重写 OnPropertyChanged，在批量操作期间抑制属性变更通知。
        /// </summary>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                base.OnPropertyChanged(e);
            }
        }
    }
}

