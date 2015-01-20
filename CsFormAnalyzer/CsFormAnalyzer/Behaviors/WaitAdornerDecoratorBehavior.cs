using CsFormAnalyzer.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace CsFormAnalyzer.Behaviors
{
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
			}

			//var operations = d.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)(() =>
			//{
			//    var adornerLayer = AdornerLayer.GetAdornerLayer(bh.AssociatedObject);
			//    if (adornerLayer != null) adornerLayer.Add(bh.waitAdorner);
			//}));
		}

		private WaitAdorner waitAdorner;

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
	}
}