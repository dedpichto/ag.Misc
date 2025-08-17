public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ParentRowViewModel> ParentRows { get; set; }
    
    public MainViewModel()
    {
        ParentRows = new ObservableCollection<ParentRowViewModel>();
    }
}

public class ParentRowViewModel : INotifyPropertyChanged
{
    public string ParentProperty1 { get; set; }
    public string ParentProperty2 { get; set; }
    
    // Dynamic child data
    public ObservableCollection<dynamic> ChildData { get; set; }
    public ObservableCollection<ColumnDefinition> ChildColumns { get; set; }
    
    public ParentRowViewModel()
    {
        ChildData = new ObservableCollection<dynamic>();
        ChildColumns = new ObservableCollection<ColumnDefinition>();
    }
    
    // Method to update child data dynamically
    public void UpdateChildData(List<Dictionary<string, object>> newData, List<string> headers)
    {
        // Clear existing data
        ChildData.Clear();
        ChildColumns.Clear();
        
        // Add new column definitions
        foreach (var header in headers)
        {
            ChildColumns.Add(new ColumnDefinition 
            { 
                Header = header, 
                PropertyName = header 
            });
        }
        
        // Add new data rows
        foreach (var row in newData)
        {
            var expandoObj = new ExpandoObject() as IDictionary<string, object>;
            foreach (var kvp in row)
            {
                expandoObj[kvp.Key] = kvp.Value;
            }
            ChildData.Add(expandoObj);
        }
    }
}

public class ColumnDefinition
{
    public string Header { get; set; }
    public string PropertyName { get; set; }
    public string DataType { get; set; } // Optional for formatting
}



<DataGrid Name="ParentDataGrid" ItemsSource="{Binding ParentRows}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Parent Property 1" Binding="{Binding ParentProperty1}" />
        <DataGridTextColumn Header="Parent Property 2" Binding="{Binding ParentProperty2}" />
    </DataGrid.Columns>
    
    <DataGrid.RowDetailsTemplate>
        <DataTemplate>
            <DataGrid Name="ChildDataGrid" 
                      ItemsSource="{Binding ChildData}"
                      AutoGenerateColumns="False"
                      MaxHeight="200"
                      local:DataGridHelper.ColumnsSource="{Binding ChildColumns}">
            </DataGrid>
        </DataTemplate>
    </DataGrid.RowDetailsTemplate>
</DataGrid>



public static class DataGridHelper
{
    public static readonly DependencyProperty ColumnsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ColumnsSource",
            typeof(ObservableCollection<ColumnDefinition>),
            typeof(DataGridHelper),
            new PropertyMetadata(null, ColumnsSourceChanged));

    public static void SetColumnsSource(DependencyObject obj, ObservableCollection<ColumnDefinition> value)
    {
        obj.SetValue(ColumnsSourceProperty, value);
    }

    public static ObservableCollection<ColumnDefinition> GetColumnsSource(DependencyObject obj)
    {
        return (ObservableCollection<ColumnDefinition>)obj.GetValue(ColumnsSourceProperty);
    }

    private static void ColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = d as DataGrid;
        if (dataGrid == null) return;

        dataGrid.Columns.Clear();

        if (e.NewValue is ObservableCollection<ColumnDefinition> columns)
        {
            foreach (var column in columns)
            {
                var dataGridColumn = new DataGridTextColumn
                {
                    Header = column.Header,
                    Binding = new Binding(column.PropertyName)
                };
                dataGrid.Columns.Add(dataGridColumn);
            }

            // Handle collection changes
            columns.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    dataGrid.Columns.Clear();
                }
                else if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (ColumnDefinition newColumn in args.NewItems)
                    {
                        var dataGridColumn = new DataGridTextColumn
                        {
                            Header = newColumn.Header,
                            Binding = new Binding(newColumn.PropertyName)
                        };
                        dataGrid.Columns.Add(dataGridColumn);
                    }
                }
            };
        }
    }
}



// When loading new data
public async Task LoadDataAsync()
{
    // Your data loading logic here
    var parentData = await GetParentDataAsync();
    
    foreach (var parent in parentData)
    {
        var parentVM = new ParentRowViewModel
        {
            ParentProperty1 = parent.Property1,
            ParentProperty2 = parent.Property2
        };
        
        // Load dynamic child data
        var childDataResult = await GetChildDataAsync(parent.Id);
        var childHeaders = childDataResult.Headers; // List<string>
        var childRows = childDataResult.Rows; // List<Dictionary<string, object>>
        
        // Update child data dynamically
        parentVM.UpdateChildData(childRows, childHeaders);
        
        ParentRows.Add(parentVM);
    }
}


public class ParentRowViewModel : INotifyPropertyChanged
{
    public DataTable ChildDataTable { get; set; }
    
    public void UpdateChildDataTable(List<Dictionary<string, object>> data, List<string> headers)
    {
        ChildDataTable = new DataTable();
        
        // Add columns
        foreach (var header in headers)
        {
            ChildDataTable.Columns.Add(header);
        }
        
        // Add rows
        foreach (var row in data)
        {
            var dataRow = ChildDataTable.NewRow();
            foreach (var kvp in row)
            {
                if (ChildDataTable.Columns.Contains(kvp.Key))
                    dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
            }
            ChildDataTable.Rows.Add(dataRow);
        }
        
        OnPropertyChanged(nameof(ChildDataTable));
    }
}

<DataGrid ItemsSource="{Binding ChildDataTable}" AutoGenerateColumns="True" />
