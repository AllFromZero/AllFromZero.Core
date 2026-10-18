using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AllFromZero.Core.Collections
{
    /// <summary>
    /// 扩展的 ObservableCollection，支持批量添加元素。
    /// 通过将范围操作拆解为多次单元素通知，完美兼容 WPF 的 ListCollectionView。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZeroObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// 用于标记是否正在执行批量操作，防止在批量操作期间触发多次通知。
        /// </summary>
        private bool _suppressNotification;

        public ZeroObservableCollection() : base() { }
        public ZeroObservableCollection(IEnumerable<T> collection) : base(collection) { }
        public ZeroObservableCollection(List<T> list) : base(list) { }

        /// <summary>
        /// 批量添加元素。内部静默添加，然后为每个元素触发单次 Add 通知。
        /// </summary>
        /// <param name="collection">要添加的元素集合。</param>
        public void AddRange(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            // 转为 List 确保能获取 Count 和索引
            var newItems = collection as IList<T> ?? [.. collection];
            if (newItems.Count == 0) return;

            // 记录添加前的起始索引
            int startIndex = Count;

            // 1. 挂起通知，内部批量添加
            _suppressNotification = true;
            try
            {
                foreach (var item in newItems)
                {
                    Items.Add(item);
                }
            }
            finally
            {
                // 恢复通知
                _suppressNotification = false;
            }

            // 2. 循环触发单元素的 Add 通知（WPF 仅支持单元素增量通知）
            // 注：WPF 的虚拟化面板对快速连续的 Add 有优化，不会像 Reset 那样全量重建
            for (int i = 0; i < newItems.Count; i++)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    newItems[i],
                    startIndex + i));
            }

            // 3. 通知 UI 更新 Count 属性
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