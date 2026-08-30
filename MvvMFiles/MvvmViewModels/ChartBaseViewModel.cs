using System;
using System.Collections.Generic;

namespace ShayCommon.Mvvm.ViewModels
{
    public abstract class ChartBaseViewModel : BaseViewModelWithActions
    {
        private readonly Dictionary<Guid, Action<string>> _saveChartByGuidActions = new();
        private readonly Dictionary<Enum, Action<string>> _saveChartActions = new();

        #region Guid register procedures
        public void RegisterSaveChartAction(Guid guid, Action<string> action)
        {
            _saveChartByGuidActions[guid] = action;
        }
        #endregion

        #region Enum register procedures
        public void RegisterSaveChartAction(Enum en, Action<string> action)
        {
            _saveChartActions[en] = action;
        }
        #endregion

        #region Guid procedures
        protected void SaveChart(Guid guid, string fileName)
        {
            if (_saveChartByGuidActions.TryGetValue(guid, out var action))
                action.Invoke(fileName);
        }
        #endregion

        #region Enum procedures
        protected void SaveChart(Enum en, string fileName)
        {
            if (_saveChartActions.TryGetValue(en, out var action))
                action.Invoke(fileName);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _saveChartByGuidActions.Clear();

                _saveChartActions.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
