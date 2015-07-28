using CsFormAnalyzer.Controls;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace CsFormAnalyzer.Behaviours
{
	/// <summary>
	/// UI요소의 최상위 레이어계층에 Wait 상태를 나타내기 위한 Behavior를 지원합니다.
	/// </summary>
	public class WaitAdornerDecoratorBehavior : Behavior<FrameworkElement>
	{
		public static readonly DependencyProperty IsActiveProperty
			= DependencyProperty.Register("IsActive", typeof(bool), typeof(WaitAdornerDecoratorBehavior), new PropertyMetadata(false, OnIsActiveChanged));

		public bool IsActive
		{
			get { return (bool)GetValue(IsActiveProperty); }
			set { SetValue(IsActiveProperty, value); }
		}

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((WaitAdornerDecoratorBehavior)d).OnIsActiveChanged();
		}

		private void OnIsActiveChanged()
		{
			if (this.AssociatedObject == null) return;
			var adornerLayer = AdornerLayer.GetAdornerLayer(this.AssociatedObject);

			if (adornerLayer != null)
			{
				if (this.IsActive)
					adornerLayer.Add(this.waitAdorner);
				else
					adornerLayer.Remove(this.waitAdorner);

				flagFirstApply = true;
			}
			else
			{
				var operations = this.AssociatedObject.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)(() =>
				{
					if (flagFirstApply) return;
					OnIsActiveChanged();
				}));
			}


		}

		private WaitAdorner waitAdorner; // wait ui
		private bool flagFirstApply; // 최초 적용 확인을 위한 플래그

		protected override void OnAttached()
		{
			AssociatedObject.SetBinding(WaitAdornerDecoratorBehavior.IsActiveProperty, new Binding() { Source = this });

			waitAdorner = new WaitAdorner(AssociatedObject);
			OnIsActiveChanged();
		}

		protected override void OnDetaching()
		{
			IsActive = false;
			OnIsActiveChanged();
		}

		public class WaitAdorner : Adorner
		{
			private FrameworkElement waitElement;

			VisualCollection visualChildren;
			protected override int VisualChildrenCount { get { return visualChildren.Count; } }
			protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

			// Arrange the Adorners.
			protected override Size ArrangeOverride(Size finalSize)
			{
				double adornerWidth = AdornedElement.RenderSize.Width;
				double adornerHeight = AdornedElement.RenderSize.Height;

				waitElement.Arrange(new Rect(0, 0, adornerWidth, adornerHeight));

				return base.ArrangeOverride(finalSize);
			}

			public WaitAdorner(UIElement adornedElement)
				: base(adornedElement)
			{
				visualChildren = new VisualCollection(this);

				var b = BindingOperations.GetBinding(adornedElement, WaitAdornerDecoratorBehavior.IsActiveProperty);
				var pr = new ProgressRing();
				pr.SetBinding(ProgressRing.IsActiveProperty, new Binding() { Source = b.Source, Path = new PropertyPath(WaitAdornerDecoratorBehavior.IsActiveProperty) });
				var board = new Border()
				{
					Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30000000")),
					Cursor = Cursors.Wait,
					Child = pr,
				};

				waitElement = board;
				visualChildren.Add(waitElement);
			}
		}

        /// <summary>
        /// 비주얼요소에 WaitAdorner를 표시합니다.
        /// </summary>
        public static void VisualAttached(Visual visual, bool value)
        {
            var behavior = Interaction.GetBehaviors(visual).Where(p => p.GetType().Equals(typeof(Behaviours.WaitAdornerDecoratorBehavior))).FirstOrDefault() as Behaviours.WaitAdornerDecoratorBehavior;
            if (behavior == null)
            {
                behavior = new WaitAdornerDecoratorBehavior();
                Interaction.GetBehaviors(visual).Add(behavior);
            }

            visual.Dispatcher.BeginInvoke(new Action(() =>
            {
                behavior.IsActive = value;
            }));
        }
    }
}