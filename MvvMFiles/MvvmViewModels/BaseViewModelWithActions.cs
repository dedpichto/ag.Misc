using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ShayCommon.Mvvm.ViewModels
{
    public abstract class BaseViewModelWithActions : BaseViewModel, IDisposable
    {
        #region Guid dictionaries
        private readonly Dictionary<Guid, Action<DependencyProperty>> _updateTargetByGuidActions = new();
        private readonly Dictionary<Guid, Func<object>> _getSelectedItemByGuidFunctions = new();
        private readonly Dictionary<Guid, Func<FlowDocument>> _getFlowDocumentByGuidFunctions = new();
        private readonly Dictionary<Guid, Action<ContextMenu>> _setDataGridContextMenuByGuidActions = new();
        private readonly Dictionary<Guid, Action<double>> _setDataGridHeightByGuidActions = new();
        private readonly Dictionary<Guid, Func<object, object>> _getDataGridContainerFromByGuidFunctions = new();
        private readonly Dictionary<Guid, Action<int, ListSortDirection>> _setSortColumnByIndexByGuidActions = new();
        private readonly Dictionary<Guid, Action> _clearDataGridSortDirectionsByGuidActions = new();
        private readonly Dictionary<Guid, Action<IEnumerable<DataGridLength>>> _setDataGridComnsWidthByGuidActions = new();
        private readonly Dictionary<Guid, Action<UIElement>> _addPanelChildByGuidActions = new();
        private readonly Dictionary<Guid, Action<UIElement>> _removePanelChildByGuidActions = new();
        private readonly Dictionary<Guid, Action> _clearPanelChildrenByGuidActions = new();
        private readonly Dictionary<Guid, Action<int>> _setSelectedIndexByGuidActions = new();
        private readonly Dictionary<Guid, Func<Control>> _getControlByGuidFunctions = new();
        private readonly Dictionary<Guid, Func<int>> _getGridViewInvisibleColumnsCountByGuidFunctions = new();
        private readonly Dictionary<Guid, Func<List<(string, double)>>> _getGridViewColumnsHeadersByGuidFunctions = new();
        private readonly Dictionary<Guid, Action<IEnumerable<string>>> _setListViewColumnsHeadersByGuidActions = new();
        private readonly Dictionary<Guid, Action<IEnumerable<object>>> _setItemsSourceByGuidActions = new();
        private readonly Dictionary<Guid, Func<string>> _getSelectedGroupMembersByGuidFunctions = new();
        private readonly Dictionary<Guid, Action> _printFlowDocumentByGuidActions = new();
        private readonly Dictionary<Guid, Action<IEnumerable<DataGridColumn>>> _replaceColumnsByGuidActions = new();
        private readonly Dictionary<Guid, Action<IEnumerable<DataGridColumn>>> _addColumnsByGuidActions = new();
        private readonly Dictionary<Guid, Action<IEnumerable<GridViewColumn>>> _addListColumnsByGuidActions = new();
        private readonly Dictionary<Guid, Action> _clearColumnsByGuidActions = new();
        private readonly Dictionary<Guid, Action<int>> _clearColumnsTillByGuidActions = new();
        private readonly Dictionary<Guid, Action<RoutedEventHandler, RoutedEventHandler>> _unhandleColumnsCheckBoxesByGuidActions = new();
        private readonly Dictionary<Guid, Func<object, object>> _getRowItemByGuidFunctions = new();
        private readonly Dictionary<Guid, Action<int, string>> _setColumnHeaderByGuidActions = new();
        private readonly Dictionary<Guid, Func<object, int>> _getColumnIndexByGuidActions = new();
        private readonly Dictionary<Guid, Action<int, DataTemplate>> _setColumnHeaderTemplateByGuidActions = new();
        private readonly Dictionary<Guid, Action<int, DataTemplate>> _setCellTemplateByGuidActions = new();
        private readonly Dictionary<Guid, Action<int>> _removeColumnByGuidActions = new();
        private readonly Dictionary<Guid, Action<int, DataGridColumn>> _insertColumnByGuidActions = new();
        private readonly Dictionary<Guid, Action<object, ListSortDirection>> _setSortColumnByTagByGuidActions = new();
        private readonly Dictionary<Guid, Action<object>> _setSelectedItemByGuidActions = new();
        private readonly Dictionary<Guid, Action<ScrollChangedEventArgs>> _scrollHorizontallyByGuidActions = new();
        private readonly Dictionary<Guid, Action<object>> _scrollIntoViewByGuidActions = new();
        private readonly Dictionary<Guid, Action<string>> _setColumnsVisibilityByGuidActions = new();
        private readonly Dictionary<Guid, Func<IEnumerable<(int, string)>>> _getVisibleColumnsByGuidFunctions = new();
        private readonly Dictionary<Guid, Func<DataGridColumn>> _getCurrentColumnByGuidFunctions = new();
        private readonly Dictionary<Guid, Action> _refreshDateStatusByGuidActions = new();
        private readonly Dictionary<Guid, Action> _clearGroupsByGuidActions = new();
        private readonly Dictionary<Guid, Action<int>> _setListViewSelectedIndexByGuidActions = new();
        #endregion

        #region Enum dictionaries
        private readonly Dictionary<Enum, Action<DependencyProperty>> _updateTargetActions = new();
        private readonly Dictionary<Enum, Func<object>> _getSelectedItemFunctions = new();
        private readonly Dictionary<Enum, Func<FlowDocument>> _getFlowDocumentFunctions = new();
        private readonly Dictionary<Enum, Action<ContextMenu>> _setDataGridContextMenuActions = new();
        private readonly Dictionary<Enum, Action<double>> _setDataGridHeightActions = new();
        private readonly Dictionary<Enum, Func<object, object>> _getDataGridContainerFromFunctions = new();
        private readonly Dictionary<Enum, Action<int, ListSortDirection>> _setSortColumnByIndexActions = new();
        private readonly Dictionary<Enum, Action> _clearDataGridSortDirectionsActions = new();
        private readonly Dictionary<Enum, Action<IEnumerable<DataGridLength>>> _setDataGridComnsWidthActions = new();
        private readonly Dictionary<Enum, Action<UIElement>> _addPanelChildActions = new();
        private readonly Dictionary<Enum, Action<UIElement>> _removePanelChildActions = new();
        private readonly Dictionary<Enum, Action> _clearPanelChildrenActions = new();
        private readonly Dictionary<Enum, Action<int>> _setSelectedIndexActions = new();
        private readonly Dictionary<Enum, Func<Control>> _getControlFunctions = new();
        private readonly Dictionary<Enum, Func<int>> _getGridViewInvisibleColumnsCountFunctions = new();
        private readonly Dictionary<Enum, Func<List<(string, double)>>> _getGridViewColumnsHeadersFunctions = new();
        private readonly Dictionary<Enum, Action<IEnumerable<string>>> _setListViewColumnsHeadersActions = new();
        private readonly Dictionary<Enum, Action<IEnumerable<object>>> _setItemsSourceActions = new();
        private readonly Dictionary<Enum, Func<string>> _getSelectedGroupMembersFunctions = new();
        private readonly Dictionary<Enum, Action> _printFlowDocumentActions = new();
        private readonly Dictionary<Enum, Action<IEnumerable<DataGridColumn>>> _replaceColumnsActions = new();
        private readonly Dictionary<Enum, Action<IEnumerable<DataGridColumn>>> _addColumnsActions = new();
        private readonly Dictionary<Enum, Action<IEnumerable<GridViewColumn>>> _addListColumnsActions = new();
        private readonly Dictionary<Enum, Action> _clearColumnsActions = new();
        private readonly Dictionary<Enum, Action<int>> _clearColumnsTillActions = new();
        private readonly Dictionary<Enum, Action<RoutedEventHandler, RoutedEventHandler>> _unhandleColumnsCheckBoxesActions = new();
        private readonly Dictionary<Enum, Func<object, object>> _getRowItemFunctions = new();
        private readonly Dictionary<Enum, Action<int, string>> _setColumnHeaderActions = new();
        private readonly Dictionary<Enum, Func<object, int>> _getColumnIndexActions = new();
        private readonly Dictionary<Enum, Action<int, DataTemplate>> _setColumnHeaderTemplateActions = new();
        private readonly Dictionary<Enum, Action<int, DataTemplate>> _setCellTemplateActions = new();
        private readonly Dictionary<Enum, Action<int>> _removeColumnActions = new();
        private readonly Dictionary<Enum, Action<int, DataGridColumn>> _insertColumnActions = new();
        private readonly Dictionary<Enum, Action<object, ListSortDirection>> _setSortColumnByTagActions = new();
        private readonly Dictionary<Enum, Action<object>> _setSelectedItemActions = new();
        private readonly Dictionary<Enum, Action<ScrollChangedEventArgs>> _scrollHorizontallyActions = new();
        private readonly Dictionary<Enum, Action<object>> _scrollIntoViewActions = new();
        private readonly Dictionary<Enum, Action<string>> _setColumnsVisibilityActions = new();
        private readonly Dictionary<Enum, Func<IEnumerable<(int, string)>>> _getVisibleColumnsFunctions = new();
        private readonly Dictionary<Enum, Func<DataGridColumn>> _getCurrentColumnFunctions = new();
        private readonly Dictionary<Enum, Action> _refreshDateStatusActions = new();
        private readonly Dictionary<Enum, Action> _clearGroupsActions = new();
        private readonly Dictionary<Enum, Action<int>> _setListViewSelectedIndexActions = new();
        #endregion

        private bool _disposedValue;

        #region Guid register procedures
        public void RegisterUpdateTargetAction(Guid guid, Action<DependencyProperty> action)
        {
            _updateTargetByGuidActions[guid] = action;
        }
        public void RegisterGetSelectedItemFunction(Guid guid, Func<object> func)
        {
            _getSelectedItemByGuidFunctions[guid] = func;
        }
        public void RegisterGetFlowDocumentFunction(Guid guid, Func<FlowDocument> func)
        {
            _getFlowDocumentByGuidFunctions[guid] = func;
        }
        public void RegisterSetDataGridContextMenuAction(Guid guid, Action<ContextMenu> action)
        {
            _setDataGridContextMenuByGuidActions[guid] = action;
        }
        public void RegisterSetDataGridHeightAction(Guid guid, Action<double> action)
        {
            _setDataGridHeightByGuidActions[guid] = action;
        }
        public void RegisterGetDataGridContainerFromFunction(Guid guid, Func<object, object> func)
        {
            _getDataGridContainerFromByGuidFunctions[guid] = func;
        }
        public void RegisterSetSortColumnByIndexAction(Guid guid, Action<int, ListSortDirection> action)
        {
            _setSortColumnByIndexByGuidActions[guid] = action;
        }
        public void RegisterClearDataGridSortDirectionsAction(Guid guid, Action action)
        {
            _clearDataGridSortDirectionsByGuidActions[guid] = action;
        }
        public void RegisterSetDataGridColumnsWidthAction(Guid guid, Action<IEnumerable<DataGridLength>> action)
        {
            _setDataGridComnsWidthByGuidActions[guid] = action;
        }
        public void RegisterAddPanelChildAction(Guid guid, Action<UIElement> action)
        {
            _addPanelChildByGuidActions[guid] = action;
        }
        public void RegisterRemovePanelChildAction(Guid guid, Action<UIElement> action)
        {
            _removePanelChildByGuidActions[guid] = action;
        }
        public void RegisterClearPanelChildrenAction(Guid guid, Action action)
        {
            _clearPanelChildrenByGuidActions[guid] = action;
        }
        public void RegisterSetSelectedIndexAction(Guid guid, Action<int> action)
        {
            _setSelectedIndexByGuidActions[guid] = action;
        }
        public void RegisterGetControlFunction(Guid guid, Func<Control> func)
        {
            _getControlByGuidFunctions[guid] = func;
        }
        public void RegisterGetInvisibleGridViewColumnsCountFunction(Guid guid, Func<int> func)
        {
            _getGridViewInvisibleColumnsCountByGuidFunctions[guid] = func;
        }
        public void RegisterGetGridViewColumnsHeadersFunction(Guid guid, Func<List<(string, double)>> func)
        {
            _getGridViewColumnsHeadersByGuidFunctions[guid] = func;
        }
        public void RegisterSetListViewColumnsHeadersAction(Guid guid, Action<IEnumerable<string>> action)
        {
            _setListViewColumnsHeadersByGuidActions[guid] = action;
        }
        public void RegisterSetItemsSourceAction(Guid guid, Action<IEnumerable<object>> action)
        {
            _setItemsSourceByGuidActions[guid] = action;
        }
        public void RegisterGetSelectedGroupMembersFunction(Guid guid, Func<string> func)
        {
            _getSelectedGroupMembersByGuidFunctions[guid] = func;
        }
        public void RegisterSetListViewSelectedIndexAction(Guid guid, Action<int> action)
        {
            _setListViewSelectedIndexByGuidActions[guid] = action;
        }
        public void RegisterClearGroupsAction(Guid guid, Action action)
        {
            _clearGroupsByGuidActions[guid] = action;
        }
        public void RegisterRefreshDateStatusAction(Guid guid, Action action)
        {
            _refreshDateStatusByGuidActions[guid] = action;
        }
        public void RegisterGetCurrentColumnFunction(Guid guid, Func<DataGridColumn> func)
        {
            _getCurrentColumnByGuidFunctions[guid] = func;
        }
        public void RegisterGetVisibleColumnsFunction(Guid guid, Func<IEnumerable<(int, string)>> func)
        {
            _getVisibleColumnsByGuidFunctions[guid] = func;
        }
        public void RegisterColumnsVisibilityAction(Guid guid, Action<string> action)
        {
            _setColumnsVisibilityByGuidActions[guid] = action;
        }
        public void RegisterPrintAction(Guid guid, Action action)
        {
            _printFlowDocumentByGuidActions[guid] = action;
        }
        public void RegisterGetColumnIndexFunc(Guid guid, Func<object, int> func)
        {
            _getColumnIndexByGuidActions[guid] = func;
        }
        public void RegisterAddColumnsAction(Guid guid, Action<IEnumerable<DataGridColumn>> action)
        {
            _addColumnsByGuidActions[guid] = action;
        }
        public void RegisterAddListColumnsAction(Guid guid, Action<IEnumerable<GridViewColumn>> action)
        {
            _addListColumnsByGuidActions[guid] = action;
        }
        public void RegisterReplaceColumnsAction(Guid guid, Action<IEnumerable<DataGridColumn>> action)
        {
            _replaceColumnsByGuidActions[guid] = action;
        }
        public void RegisterClearColumnsAction(Guid guid, Action action)
        {
            _clearColumnsByGuidActions[guid] = action;
        }
        public void RegisterClearColumnsTillAction(Guid guid, Action<int> action)
        {
            _clearColumnsTillByGuidActions[guid] = action;
        }
        public void RegisterUnhandleColumnsCheckboxesAction(Guid guid, Action<RoutedEventHandler, RoutedEventHandler> action)
        {
            _unhandleColumnsCheckBoxesByGuidActions[guid] = action;
        }
        public void RegisterGetRowItemFunc(Guid guid, Func<object, object> func)
        {
            _getRowItemByGuidFunctions[guid] = func;
        }
        public void RegisterSetColumnHeaderAction(Guid guid, Action<int, string> action)
        {
            _setColumnHeaderByGuidActions[guid] = action;
        }
        public void RegisterSetColumnHeaderTemplateAction(Guid guid, Action<int, DataTemplate> action)
        {
            _setColumnHeaderTemplateByGuidActions[guid] = action;
        }
        public void RegisterSetCellTemplateAction(Guid guid, Action<int, DataTemplate> action)
        {
            _setCellTemplateByGuidActions[guid] = action;
        }
        public void RegisterRemoveColumnAction(Guid guid, Action<int> action)
        {
            _removeColumnByGuidActions[guid] = action;
        }
        public void RegisterInsertColumnAction(Guid guid, Action<int, DataGridColumn> action)
        {
            _insertColumnByGuidActions[guid] = action;
        }
        public void RegisterSetSortColumnByTagAction(Guid guid, Action<object, ListSortDirection> action)
        {
            _setSortColumnByTagByGuidActions[guid] = action;
        }
        public void RegisterSetSelectedItemAction(Guid guid, Action<object> action)
        {
            _setSelectedItemByGuidActions[guid] = action;
        }
        public void RegisterScrollHorizontallyAction(Guid guid, Action<ScrollChangedEventArgs> action)
        {
            _scrollHorizontallyByGuidActions[guid] = action;
        }
        public void RegisterScrollIntoAction(Guid guid, Action<object> action)
        {
            _scrollIntoViewByGuidActions[guid] = action;
        }
        #endregion

        #region Enum register procedures
        public void RegisterUpdateTargetAction(Enum enumValue, Action<DependencyProperty> action)
        {
            _updateTargetActions[enumValue] = action;
        }
        public void RegisterGetSelectedItemFunction(Enum enumValue, Func<object> func)
        {
            _getSelectedItemFunctions[enumValue] = func;
        }
        public void RegisterGetFlowDocumentFunction(Enum enumValue, Func<FlowDocument> func)
        {
            _getFlowDocumentFunctions[enumValue] = func;
        }
        public void RegisterSetDataGridContextMenuAction(Enum enumValue, Action<ContextMenu> action)
        {
            _setDataGridContextMenuActions[enumValue] = action;
        }
        public void RegisterSetDataGridHeightAction(Enum enumValue, Action<double> action)
        {
            _setDataGridHeightActions[enumValue] = action;
        }
        public void RegisterGetDataGridContainerFromFunction(Enum enumValue, Func<object, object> func)
        {
            _getDataGridContainerFromFunctions[enumValue] = func;
        }
        public void RegisterSetSortColumnByIndexAction(Enum enumValue, Action<int, ListSortDirection> action)
        {
            _setSortColumnByIndexActions[enumValue] = action;
        }
        public void RegisterClearDataGridSortDirectionsAction(Enum enumValue, Action action)
        {
            _clearDataGridSortDirectionsActions[enumValue] = action;
        }
        public void RegisterSetDataGridColumnsWidthAction(Enum enumValue, Action<IEnumerable<DataGridLength>> action)
        {
            _setDataGridComnsWidthActions[enumValue] = action;
        }
        public void RegisterAddPanelChildAction(Enum enumValue, Action<UIElement> action)
        {
            _addPanelChildActions[enumValue] = action;
        }
        public void RegisterRemovePanelChildAction(Enum enumValue, Action<UIElement> action)
        {
            _removePanelChildActions[enumValue] = action;
        }
        public void RegisterClearPanelChildrenAction(Enum enumValue, Action action)
        {
            _clearPanelChildrenActions[enumValue] = action;
        }
        public void RegisterSetSelectedIndexAction(Enum enumValue, Action<int> action)
        {
            _setSelectedIndexActions[enumValue] = action;
        }
        public void RegisterGetControlFunction(Enum enumValue, Func<Control> func)
        {
            _getControlFunctions[enumValue] = func;
        }
        public void RegisterGetInvisibleGridViewColumnsCountFunction(Enum enumValue, Func<int> func)
        {
            _getGridViewInvisibleColumnsCountFunctions[enumValue] = func;
        }
        public void RegisterGetGridViewColumnsHeadersFunction(Enum enumValue, Func<List<(string, double)>> func)
        {
            _getGridViewColumnsHeadersFunctions[enumValue] = func;
        }
        public void RegisterSetListViewColumnsHeadersAction(Enum enumValue, Action<IEnumerable<string>> action)
        {
            _setListViewColumnsHeadersActions[enumValue] = action;
        }
        public void RegisterSetItemsSourceAction(Enum enumValue, Action<IEnumerable<object>> action)
        {
            _setItemsSourceActions[enumValue] = action;
        }
        public void RegisterGetSelectedGroupMembersFunction(Enum enumValue, Func<string> func)
        {
            _getSelectedGroupMembersFunctions[enumValue] = func;
        }
        public void RegisterSetListViewSelectedIndexAction(Enum enumValue, Action<int> action)
        {
            _setListViewSelectedIndexActions[enumValue] = action;
        }
        public void RegisterClearGroupsAction(Enum enumValue, Action action)
        {
            _clearGroupsActions[enumValue] = action;
        }
        public void RegisterRefreshDateStatusAction(Enum enumValue, Action action)
        {
            _refreshDateStatusActions[enumValue] = action;
        }
        public void RegisterGetCurrentColumnFunction(Enum enumValue, Func<DataGridColumn> func)
        {
            _getCurrentColumnFunctions[enumValue] = func;
        }
        public void RegisterGetVisibleColumnsFunction(Enum enumValue, Func<IEnumerable<(int, string)>> func)
        {
            _getVisibleColumnsFunctions[enumValue] = func;
        }
        public void RegisterColumnsVisibilityAction(Enum enumValue, Action<string> action)
        {
            _setColumnsVisibilityActions[enumValue] = action;
        }
        public void RegisterPrintAction(Enum enumValue, Action action)
        {
            _printFlowDocumentActions[enumValue] = action;
        }
        public void RegisterGetColumnIndexFunc(Enum enumValue, Func<object, int> func)
        {
            _getColumnIndexActions[enumValue] = func;
        }
        public void RegisterReplaceColumnsAction(Enum enumValue, Action<IEnumerable<DataGridColumn>> action)
        {
            _replaceColumnsActions[enumValue] = action;
        }
        public void RegisterAddColumnsAction(Enum enumValue, Action<IEnumerable<DataGridColumn>> action)
        {
            _addColumnsActions[enumValue] = action;
        }
        public void RegisterAddListColumnsAction(Enum enumValue, Action<IEnumerable<GridViewColumn>> action)
        {
            _addListColumnsActions[enumValue] = action;
        }
        public void RegisterClearColumnsAction(Enum enumValue, Action action)
        {
            _clearColumnsActions[enumValue] = action;
        }
        public void RegisterClearColumnsTillAction(Enum enumValue, Action<int> action)
        {
            _clearColumnsTillActions[enumValue] = action;
        }
        public void RegisterUnhandleColumnsCheckboxesAction(Enum enumValue, Action<RoutedEventHandler, RoutedEventHandler> action)
        {
            _unhandleColumnsCheckBoxesActions[enumValue] = action;
        }
        public void RegisterGetRowItemFunc(Enum enumValue, Func<object, object> func)
        {
            _getRowItemFunctions[enumValue] = func;
        }
        public void RegisterSetColumnHeaderAction(Enum enumValue, Action<int, string> action)
        {
            _setColumnHeaderActions[enumValue] = action;
        }
        public void RegisterSetColumnHeaderTemplateAction(Enum enumValue, Action<int, DataTemplate> action)
        {
            _setColumnHeaderTemplateActions[enumValue] = action;
        }
        public void RegisterSetCellTemplateAction(Enum enumValue, Action<int, DataTemplate> action)
        {
            _setCellTemplateActions[enumValue] = action;
        }
        public void RegisterRemoveColumnAction(Enum enumValue, Action<int> action)
        {
            _removeColumnActions[enumValue] = action;
        }
        public void RegisterInsertColumnAction(Enum enumValue, Action<int, DataGridColumn> action)
        {
            _insertColumnActions[enumValue] = action;
        }
        public void RegisterSetSortColumnByTagAction(Enum enumValue, Action<object, ListSortDirection> action)
        {
            _setSortColumnByTagActions[enumValue] = action;
        }
        public void RegisterSetSelectedItemAction(Enum enumValue, Action<object> action)
        {
            _setSelectedItemActions[enumValue] = action;
        }
        public void RegisterScrollHorizontallyAction(Enum enumValue, Action<ScrollChangedEventArgs> action)
        {
            _scrollHorizontallyActions[enumValue] = action;
        }
        public void RegisterScrollIntoAction(Enum enumValue, Action<object> action)
        {
            _scrollIntoViewActions[enumValue] = action;
        }
        #endregion

        #region Guid procedures
        protected void UpdateTarget(Guid guid, DependencyProperty dependencyProperty)
        {
            if (_updateTargetByGuidActions.TryGetValue(guid, out var action))
                action.Invoke(dependencyProperty);
        }
        protected object GetSelectedItem(Guid guid)
        {

            if (_getSelectedItemByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return null;
        }
        protected FlowDocument GetFlowDocument(Guid guid)
        {
            if (_getFlowDocumentByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return null;
        }
        protected void SetDataGridContextMenu(Guid guid,ContextMenu contextMenu)
        {
            if (_setDataGridContextMenuByGuidActions.TryGetValue(guid, out var action))
                action.Invoke(contextMenu);
        }
        protected void SetDataGridHeight(Guid guid, double value)
        {
            if (_setDataGridHeightByGuidActions.TryGetValue(guid, out var action))
                action.Invoke(value);
        }
        protected object GetDataGridContainerFrom(Guid guid, object parameter)
        {
            if (_getDataGridContainerFromByGuidFunctions.TryGetValue(guid, out var func))
                return func(parameter);
            return null;
        }
        protected void SetDataGridSortColumnByIndex(Guid guid, int index, ListSortDirection direction)
        {
            if (_setSortColumnByIndexByGuidActions.TryGetValue(guid, out var action))
                action(index, direction);
        }
        protected void ClearDataGridSortDirections(Guid guid)
        {
            if (_clearDataGridSortDirectionsByGuidActions.TryGetValue(guid, out var action))
                action();
        }
        protected void SetDataGridColumnsWidth(Guid guid, IEnumerable<DataGridLength> headersWidth)
        {
            if (_setDataGridComnsWidthByGuidActions.TryGetValue(guid, out var action))
                action(headersWidth);
        }
        protected void AddPanelChild(Guid guid, UIElement child)
        {
            if (_addPanelChildByGuidActions.TryGetValue(guid, out var action))
                action(child);
        }
        protected void RemovePanelChild(Guid guid, UIElement child)
        {
            if (_removePanelChildByGuidActions.TryGetValue(guid, out var action))
                action(child);
        }
        protected void ClearPanelChildren(Guid guid)
        {
            if (_clearPanelChildrenByGuidActions.TryGetValue(guid, out var action))
                action();
        }
        protected void SetSelectedIndex(Guid guid, int index)
        {
            if (_setSelectedIndexByGuidActions.TryGetValue(guid, out var action))
                action(index);
        }
        protected Control GetControl(Guid guid)
        {
            if (_getControlByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return null;
        }
        protected int GetInvisibleGridViewColumnsCount(Guid guid)
        {
            if (_getGridViewInvisibleColumnsCountByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return 0;
        }
        protected List<(string, double)> GetGridViewColumnsHeaders(Guid guid)
        {
            if (_getGridViewColumnsHeadersByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return null;
        }
        protected void SetListViewColumnsHeaders(Guid guid, IEnumerable<string> headers)
        {
            if (_setListViewColumnsHeadersByGuidActions.TryGetValue(guid, out var action))
                action(headers);
        }
        protected void SetItemsSource(Guid guid, IEnumerable<object> source)
        {
            if (_setItemsSourceByGuidActions.TryGetValue(guid, out var action))
                action(source);
        }
        protected string GetSelectedGroupMembers(Guid guid)
        {
            if (_getSelectedGroupMembersByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return "";
        }
        protected void SetListViewSelectedIndex(Guid guid, int index)
        {
            if (_setListViewSelectedIndexByGuidActions.TryGetValue(guid, out var action))
                action(index);
        }
        protected void ClearGroups(Guid guid)
        {
            if (_clearGroupsByGuidActions.TryGetValue(guid, out var action))
                action();
        }
        protected void RefreshDatesStatus(Guid guid)
        {
            if (_refreshDateStatusByGuidActions.TryGetValue(guid, out var action))
                action();
        }
        protected DataGridColumn GetDataGridCurrentColumn(Guid guid)
        {
            if (_getCurrentColumnByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return null;
        }
        protected IEnumerable<(int, string)> GetDataGridVisibleColumns(Guid guid)
        {
            if (_getVisibleColumnsByGuidFunctions.TryGetValue(guid, out var func))
                return func();
            return null;
        }
        protected void SetDataGridColumnsVisibility(Guid guid, string filter)
        {
            if (_setColumnsVisibilityByGuidActions.TryGetValue(guid, out var action))
                action(filter);
        }
        protected void PrintFlowDocument(Guid guid)
        {
            if (_printFlowDocumentByGuidActions.TryGetValue(guid, out var action))
                action();
        }
        protected void AddDataGridColumns(Guid guid, IEnumerable<DataGridColumn> columns)
        {
            if (_addColumnsByGuidActions.TryGetValue(guid, out var action))
                action(columns);
        }
        protected void AddListViewColumns(Guid guid, IEnumerable<GridViewColumn> columns)
        {
            if (_addListColumnsByGuidActions.TryGetValue(guid, out var action))
                action(columns);
        }
        protected void ReplaceDataGridColumns(Guid guid, IEnumerable<DataGridColumn> columns)
        {
            if (_replaceColumnsByGuidActions.TryGetValue(guid, out var action))
                action(columns);
        }
        protected void ClearDataGridColumns(Guid guid)
        {
            if (_clearColumnsByGuidActions.TryGetValue(guid, out var action))
                action();
        }
        protected void ClearDataGridColumnsTill(Guid guid, int columnsToPreserve)
        {
            if (_clearColumnsTillByGuidActions.TryGetValue(guid, out var action))
                action(columnsToPreserve);
        }
        protected void UnhandleDataGridCheckBoxes(Guid guid, RoutedEventHandler checkedEvent, RoutedEventHandler uncheckedEvent)
        {
            if (_unhandleColumnsCheckBoxesByGuidActions.TryGetValue(guid, out var action))
                action(checkedEvent, uncheckedEvent);
        }
        protected object GetDataGridRowItem(Guid guid, object rowObject)
        {
            if (_getRowItemByGuidFunctions.TryGetValue(guid, out var func))
                return func(rowObject);
            return null;
        }
        protected void SetDataGridColumnHeader(Guid guid, int index, string header)
        {
            if (_setColumnHeaderByGuidActions.TryGetValue(guid, out var action))
                action(index, header);
        }
        protected int GetDataGridColumnIntex(Guid guid, object colObject)
        {
            if (_getColumnIndexByGuidActions.TryGetValue(guid, out var func))
                return func(colObject);
            return -1;
        }
        protected void SetDataGridColumnHeaderTemplate(Guid guid, int index, DataTemplate headerTemplate)
        {
            if (_setColumnHeaderTemplateByGuidActions.TryGetValue(guid, out var action))
                action(index, headerTemplate);
        }
        protected void SetDataGridCellTemplate(Guid guid, int index, DataTemplate headerTemplate)
        {
            if (_setCellTemplateByGuidActions.TryGetValue(guid, out var action))
                action(index, headerTemplate);
        }
        protected void RemoveDataGridColumn(Guid guid, int index)
        {
            if (_removeColumnByGuidActions.TryGetValue(guid, out var action))
                action(index);
        }
        protected void InsertDataGridColumn(Guid guid, int index, DataGridColumn column)
        {
            if (_insertColumnByGuidActions.TryGetValue(guid, out var action))
                action(index, column);
        }
        protected void SetDataGridSortColumnByTag(Guid guid, object tag, ListSortDirection direction)
        {
            if (_setSortColumnByTagByGuidActions.TryGetValue(guid, out var action))
                action(tag, direction);
        }
        protected void SetSelectedItem(Guid guid, object item)
        {
            if (_setSelectedItemByGuidActions.TryGetValue(guid, out var action))
                action(item);
        }
        protected void ScrollDataGridHorizontally(Guid guid, ScrollChangedEventArgs e)
        {
            if (_scrollHorizontallyByGuidActions.TryGetValue(guid, out var action))
                action(e);
        }
        protected void ScrollItemIntoView(Guid guid, object item)
        {
            if (_scrollIntoViewByGuidActions.TryGetValue(guid, out var action))
                action(item);
        }
        #endregion

        #region Enum procedures
        protected void UpdateTarget(Enum enumValue, DependencyProperty dependencyProperty)
        {
            if (_updateTargetActions.TryGetValue(enumValue, out var action))
                action.Invoke(dependencyProperty);
        }
        protected object GetSelectedItem(Enum enumValue)
        {

            if (_getSelectedItemFunctions.TryGetValue(enumValue, out var func))
                return func();
            return null;
        }
        protected FlowDocument GetFlowDocument(Enum enumValue)
        {
            if (_getFlowDocumentFunctions.TryGetValue(enumValue, out var func))
                return func();
            return null;
        }
        protected void SetDataGridContextMenu(Enum enumValue, ContextMenu contextMenu)
        {
            if (_setDataGridContextMenuActions.TryGetValue(enumValue, out var action))
                action.Invoke(contextMenu);
        }
        protected void SetDataGridHeight(Enum enumValue, double value)
        {
            if (_setDataGridHeightActions.TryGetValue(enumValue, out var action))
                action.Invoke(value);
        }
        protected object GetDataGridContainerFrom(Enum enumValue, object parameter)
        {
            if (_getDataGridContainerFromFunctions.TryGetValue(enumValue, out var func))
                return func(parameter);
            return null;
        }
        protected void SetDataGridSortColumnByIndex(Enum enumValue, int index, ListSortDirection direction)
        {
            if (_setSortColumnByIndexActions.TryGetValue(enumValue, out var action))
                action(index, direction);
        }
        protected void ClearDataGridSortDirections(Enum enumValue)
        {
            if (_clearDataGridSortDirectionsActions.TryGetValue(enumValue, out var action))
                action();
        }
        protected void SetDataGridColumnsWidth(Enum enumValue, IEnumerable<DataGridLength> headersWidth)
        {
            if (_setDataGridComnsWidthActions.TryGetValue(enumValue, out var action))
                action(headersWidth);
        }
        protected void AddPanelChild(Enum enumValue, UIElement child)
        {
            if (_addPanelChildActions.TryGetValue(enumValue, out var action))
                action(child);
        }
        protected void RemovePanelChild(Enum enumValue, UIElement child)
        {
            if (_removePanelChildActions.TryGetValue(enumValue, out var action))
                action(child);
        }
        protected void ClearPanelChildren(Enum enumValue)
        {
            if (_clearPanelChildrenActions.TryGetValue(enumValue, out var action))
                action();
        }
        protected void SetSelectedIndex(Enum enumValue, int index)
        {
            if (_setSelectedIndexActions.TryGetValue(enumValue, out var action))
                action(index);
        }
        protected Control GetControl(Enum enumValue)
        {
            if (_getControlFunctions.TryGetValue(enumValue, out var func))
                return func();
            return null;
        }
        protected int GetInvisibleGridViewColumnsCount(Enum enumValue)
        {
            if (_getGridViewInvisibleColumnsCountFunctions.TryGetValue(enumValue, out var func))
                return func();
            return 0;
        }
        protected List<(string, double)> GetGridViewColumnsHeaders(Enum enumValue)
        {
            if (_getGridViewColumnsHeadersFunctions.TryGetValue(enumValue, out var func))
                return func();
            return null;
        }
        protected void SetListViewColumnsHeaders(Enum enumValue, IEnumerable<string> headers)
        {
            if (_setListViewColumnsHeadersActions.TryGetValue(enumValue, out var action))
                action(headers);
        }
        protected void SetItemsSource(Enum enumValue, IEnumerable<object> source)
        {
            if (_setItemsSourceActions.TryGetValue(enumValue, out var action))
                action(source);
        }
        protected string GetSelectedGroupMembers(Enum enumValue)
        {
            if (_getSelectedGroupMembersFunctions.TryGetValue(enumValue, out var func))
                return func();
            return "";
        }
        protected void SetListViewSelectedIndex(Enum enumValue, int index)
        {
            if (_setListViewSelectedIndexActions.TryGetValue(enumValue, out var action))
                action(index);
        }
        protected void ClearGroups(Enum enumValue)
        {
            if (_clearGroupsActions.TryGetValue(enumValue, out var action))
                action();
        }
        protected void RefreshDatesStatus(Enum enumValue)
        {
            if (_refreshDateStatusActions.TryGetValue(enumValue, out var action))
                action();
        }
        protected DataGridColumn GetDataGridCurrentColumn(Enum enumValue)
        {
            if (_getCurrentColumnFunctions.TryGetValue(enumValue, out var func))
                return func();
            return null;
        }
        protected IEnumerable<(int, string)> GetDataGridVisibleColumns(Enum enumValue)
        {
            if (_getVisibleColumnsFunctions.TryGetValue(enumValue, out var func))
                return func();
            return null;
        }
        protected void SetDataGridColumnsVisibility(Enum enumValue, string filter)
        {
            if (_setColumnsVisibilityActions.TryGetValue(enumValue, out var action))
                action(filter);
        }
        protected void PrintFlowDocument(Enum enumValue)
        {
            if (_printFlowDocumentActions.TryGetValue(enumValue, out var action))
                action();
        }
        protected void AddDataGridColumns(Enum enumValue, IEnumerable<DataGridColumn> columns)
        {
            if (_addColumnsActions.TryGetValue(enumValue, out var action))
                action(columns);
        }
        protected void AddListViewColumns(Enum enumValue, IEnumerable<GridViewColumn> columns)
        {
            if (_addListColumnsActions.TryGetValue(enumValue, out var action))
                action(columns);
        }
        protected void ReplaceDataGridColumns(Enum enumValue, IEnumerable<DataGridColumn> columns)
        {
            if (_replaceColumnsActions.TryGetValue(enumValue, out var action))
                action(columns);
        }
        protected void ClearDataGridColumns(Enum enumValue)
        {
            if (_clearColumnsActions.TryGetValue(enumValue, out var action))
                action();
        }
        protected void ClearDataGridColumnsTill(Enum enumValue, int columnsToPreserve)
        {
            if (_clearColumnsTillActions.TryGetValue(enumValue, out var action))
                action(columnsToPreserve);
        }
        protected void UnhandleDataGridCheckBoxes(Enum enumValue, RoutedEventHandler checkedEvent, RoutedEventHandler uncheckedEvent)
        {
            if (_unhandleColumnsCheckBoxesActions.TryGetValue(enumValue, out var action))
                action(checkedEvent, uncheckedEvent);
        }
        protected object GetDataGridRowItem(Enum enumValue, object rowObject)
        {
            if (_getRowItemFunctions.TryGetValue(enumValue, out var func))
                return func(rowObject);
            return null;
        }
        protected void SetDataGridColumnHeader(Enum enumValue, int index, string header)
        {
            if (_setColumnHeaderActions.TryGetValue(enumValue, out var action))
                action(index, header);
        }
        protected int GetDataGridColumnIntex(Enum enumValue, object colObject)
        {
            if (_getColumnIndexActions.TryGetValue(enumValue, out var func))
                return func(colObject);
            return -1;
        }
        protected void SetDataGridColumnHeaderTemplate(Enum enumValue, int index, DataTemplate headerTemplate)
        {
            if (_setColumnHeaderTemplateActions.TryGetValue(enumValue, out var action))
                action(index, headerTemplate);
        }
        protected void SetDataGridCellTemplate(Enum enumValue, int index, DataTemplate headerTemplate)
        {
            if (_setCellTemplateActions.TryGetValue(enumValue, out var action))
                action(index, headerTemplate);
        }
        protected void RemoveDataGridColumn(Enum enumValue, int index)
        {
            if (_removeColumnActions.TryGetValue(enumValue, out var action))
                action(index);
        }
        protected void InsertDataGridColumn(Enum enumValue, int index, DataGridColumn column)
        {
            if (_insertColumnActions.TryGetValue(enumValue, out var action))
                action(index, column);
        }
        protected void SetDataGridSortColumnByTag(Enum enumValue, object tag, ListSortDirection direction)
        {
            if (_setSortColumnByTagActions.TryGetValue(enumValue, out var action))
                action(tag, direction);
        }
        protected void SetSelectedItem(Enum enumValue, object item)
        {
            if (_setSelectedItemActions.TryGetValue(enumValue, out var action))
                action(item);
        }
        protected void ScrollDataGridHorizontally(Enum enumValue, ScrollChangedEventArgs e)
        {
            if (_scrollHorizontallyActions.TryGetValue(enumValue, out var action))
                action(e);
        }
        protected void ScrollItemIntoView(Enum enumValue, object item)
        {
            if (_scrollIntoViewActions.TryGetValue(enumValue, out var action))
                action(item);
        }
        #endregion

        #region IDisposable implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    clearAllRegistrations();
                }
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BaseViewModelWithActions()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void clearAllRegistrations()
        {
            // Guid dictionaries
            _updateTargetByGuidActions.Clear();
            _getSelectedItemByGuidFunctions.Clear();
            _getFlowDocumentByGuidFunctions.Clear();
            _setDataGridContextMenuByGuidActions.Clear();
            _setDataGridHeightByGuidActions.Clear();
            _getDataGridContainerFromByGuidFunctions.Clear();
            _setSortColumnByIndexByGuidActions.Clear();
            _clearDataGridSortDirectionsByGuidActions.Clear();
            _setDataGridComnsWidthByGuidActions.Clear();
            _addPanelChildByGuidActions.Clear();
            _removePanelChildByGuidActions.Clear();
            _clearPanelChildrenByGuidActions.Clear();
            _setSelectedIndexByGuidActions.Clear();
            _getControlByGuidFunctions.Clear();
            _getGridViewInvisibleColumnsCountByGuidFunctions.Clear();
            _getGridViewColumnsHeadersByGuidFunctions.Clear();
            _setListViewColumnsHeadersByGuidActions.Clear();
            _setItemsSourceByGuidActions.Clear();
            _getSelectedGroupMembersByGuidFunctions.Clear();
            _printFlowDocumentByGuidActions.Clear();
            _replaceColumnsByGuidActions.Clear();
            _addColumnsByGuidActions.Clear();
            _addListColumnsByGuidActions.Clear();
            _clearColumnsByGuidActions.Clear();
            _clearColumnsTillByGuidActions.Clear();
            _unhandleColumnsCheckBoxesByGuidActions.Clear();
            _getRowItemByGuidFunctions.Clear();
            _setColumnHeaderByGuidActions.Clear();
            _getColumnIndexByGuidActions.Clear();
            _setColumnHeaderTemplateByGuidActions.Clear();
            _setCellTemplateByGuidActions.Clear();
            _removeColumnByGuidActions.Clear();
            _insertColumnByGuidActions.Clear();
            _setSortColumnByTagByGuidActions.Clear();
            _setSelectedItemByGuidActions.Clear();
            _scrollHorizontallyByGuidActions.Clear();
            _scrollIntoViewByGuidActions.Clear();
            _setColumnsVisibilityByGuidActions.Clear();
            _getVisibleColumnsByGuidFunctions.Clear();
            _getCurrentColumnByGuidFunctions.Clear();
            _refreshDateStatusByGuidActions.Clear();
            _clearGroupsByGuidActions.Clear();
            _setListViewSelectedIndexByGuidActions.Clear();

            // Enum dictionaries
            _updateTargetActions.Clear();
            _getSelectedItemFunctions.Clear();
            _getFlowDocumentFunctions.Clear();
            _setDataGridContextMenuActions.Clear();
            _setDataGridHeightActions.Clear();
            _getDataGridContainerFromFunctions.Clear();
            _setSortColumnByIndexActions.Clear();
            _clearDataGridSortDirectionsActions.Clear();
            _setDataGridComnsWidthActions.Clear();
            _addPanelChildActions.Clear();
            _removePanelChildActions.Clear();
            _clearPanelChildrenActions.Clear();
            _setSelectedIndexActions.Clear();
            _getControlFunctions.Clear();
            _getGridViewInvisibleColumnsCountFunctions.Clear();
            _getGridViewColumnsHeadersFunctions.Clear();
            _setListViewColumnsHeadersActions.Clear();
            _setItemsSourceActions.Clear();
            _getSelectedGroupMembersFunctions.Clear();
            _printFlowDocumentActions.Clear();
            _replaceColumnsActions.Clear();
            _addColumnsActions.Clear();
            _addListColumnsActions.Clear();
            _clearColumnsActions.Clear();
            _clearColumnsTillActions.Clear();
            _unhandleColumnsCheckBoxesActions.Clear();
            _getRowItemFunctions.Clear();
            _setColumnHeaderActions.Clear();
            _getColumnIndexActions.Clear();
            _setColumnHeaderTemplateActions.Clear();
            _setCellTemplateActions.Clear();
            _removeColumnActions.Clear();
            _insertColumnActions.Clear();
            _setSortColumnByTagActions.Clear();
            _setSelectedItemActions.Clear();
            _scrollHorizontallyActions.Clear();
            _scrollIntoViewActions.Clear();
            _setColumnsVisibilityActions.Clear();
            _getVisibleColumnsFunctions.Clear();
            _getCurrentColumnFunctions.Clear();
            _refreshDateStatusActions.Clear();
            _clearGroupsActions.Clear();
            _setListViewSelectedIndexActions.Clear();
        }
    }
}
