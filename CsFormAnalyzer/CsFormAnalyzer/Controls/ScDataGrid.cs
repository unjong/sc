using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CsFormAnalyzer.Controls
{
    public class ScDataGrid : DataGrid
    {
        static ScDataGrid()
        {            
            CommandManager.RegisterClassCommandBinding(
                typeof(ScDataGrid),
                new CommandBinding(ApplicationCommands.Paste,
                    new ExecutedRoutedEventHandler(OnExecutedPaste),
                    new CanExecuteRoutedEventHandler(OnCanExecutePaste)));
        }                

        public ScDataGrid()
        {
            this.MouseDoubleClick += ScDataGrid_MouseDoubleClick;
        }
        
        #region Events

        //public delegate void RowDoubleClickEventHandler(object sender, DataGridRow dataGridRow);
        //public event RowDoubleClickEventHandler RowDoubleClick;

        public static readonly RoutedEvent RowDoubleClickEvent = EventManager.RegisterRoutedEvent(
            "RowDoubleClick", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ScDataGrid));

        public event RoutedEventHandler RowDoubleClick
        {
            add { AddHandler(RowDoubleClickEvent, value); }
            remove { RemoveHandler(RowDoubleClickEvent, value); }
        }

        private void ScDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    DataGridRow dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
                    if (dgr != null)
                    {
                        RaiseEvent(new RoutedEventArgs(RowDoubleClickEvent));
                    }
                }
            }
        }

        #endregion

        #region Clipboard Paste

        private static void OnCanExecutePaste(object target, CanExecuteRoutedEventArgs args)
        {
            ((ScDataGrid)target).OnCanExecutePaste(args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Paste command query its state.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCanExecutePaste(CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = CurrentCell != null;
            args.Handled = true;
        }

        private static void OnExecutedPaste(object target, ExecutedRoutedEventArgs args)
        {
            ((ScDataGrid)target).OnExecutedPaste(args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Paste command is executed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExecutedPaste(ExecutedRoutedEventArgs args)
        {
            var cursor = this.Cursor;
            this.Cursor = Cursors.Wait;

            try
            {
                // parse the clipboard data            
                List<string[]> rowData = ClipboardHelper.ParseClipboardData();

                bool hasAddedNewRow = false;

                // call OnPastingCellClipboardContent for each cell
                int minRowIndex = Items.IndexOf(CurrentItem);
                int maxRowIndex = Items.Count - 1;
                //int minColumnDisplayIndex = (SelectionUnit != DataGridSelectionUnit.FullRow) ? Columns.IndexOf(CurrentColumn) : 0;
                int minColumnDisplayIndex = Columns.IndexOf(CurrentColumn);
                int maxColumnDisplayIndex = Columns.Count - 1;
                int rowDataIndex = 0;
                
                for (int i = minRowIndex; i <= maxRowIndex && rowDataIndex < rowData.Count; i++, rowDataIndex++)
                {
                    CurrentItem = Items[i];

                    BeginEditCommand.Execute(null, this);

                    int columnDataIndex = 0;
                    for (int j = minColumnDisplayIndex; j <= maxColumnDisplayIndex && columnDataIndex < rowData[rowDataIndex].Length; j++, columnDataIndex++)
                    {
                        DataGridColumn column = ColumnFromDisplayIndex(j);
                        column.OnPastingCellClipboardContent(Items[i], rowData[rowDataIndex][columnDataIndex]);
                    }

                    CommitEditCommand.Execute(this, this);
                    if (i == maxRowIndex)
                    {
                        maxRowIndex++;
                        hasAddedNewRow = true;
                    }
                }

                // update selection
                if (hasAddedNewRow)
                {
                    UnselectAll();
                    UnselectAllCells();

                    CurrentItem = Items[minRowIndex];

                    if (SelectionUnit == DataGridSelectionUnit.FullRow)
                    {
                        SelectedItem = Items[minRowIndex];
                    }
                    else if (SelectionUnit == DataGridSelectionUnit.CellOrRowHeader ||
                             SelectionUnit == DataGridSelectionUnit.Cell)
                    {
                        SelectedCells.Add(new DataGridCellInfo(Items[minRowIndex], Columns[minColumnDisplayIndex]));

                    }
                }
            }
            catch (Exception)
            {                
                //throw;
            }
            finally
            {
                this.Cursor = cursor;
            }
        }

        /// <summary>
        ///     Whether the end-user can add new rows to the ItemsSource.
        /// </summary>
        public bool CanUserPasteToNewRows
        {
            get { return (bool)GetValue(CanUserPasteToNewRowsProperty); }
            set { SetValue(CanUserPasteToNewRowsProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for CanUserAddRows.
        /// </summary>
        public static readonly DependencyProperty CanUserPasteToNewRowsProperty =
            DependencyProperty.Register("CanUserPasteToNewRows",
                                        typeof(bool), typeof(ScDataGrid), 
                                        new FrameworkPropertyMetadata(true, null, null));

        #endregion Clipboard Paste
    }
}
