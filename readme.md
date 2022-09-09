Your question says that you would like a row that is not checked should to be readonly. One obvious conflict is that if a row is truly read-only in its entireity, then there would be no way to ever check it to make it editable. My first suggestion would be to make a `refreshStyles` method that takes into account which cells are `DataGridViewCheckBoxCell` so that these can always be toggled regardless of the state of the row.

    private void refreshStyles()
    {
        for (int i = 0; i < DataSource.Count; i++)
        {
            var item = DataSource[i];
            var row = dataGridView.Rows[i];
            // Get all of the row cells that aren't checkboxes.
            var cells = row.Cells.Cast<DataGridViewCell>().Where(_ => !(_ is DataGridViewCheckBoxCell));
            if(item.IsChecked)
            {
                row.DefaultCellStyle.BackColor = Color.Azure;
                foreach (var cell in cells) cell.ReadOnly = false;
            }
            else
            {
                dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(0xf0, 0xf0, 0xf0);
                foreach (var cell in cells) cell.ReadOnly = true;
            }
        }
    }

This is a step towards having the DGV present with a mix of read-only and editable rows.

![screenshot](https://github.com/IVSoftware/dgv-with-row-styles/blob/master/dgv_with_row_styles/Screenshots/screenshots.png)

***
This is all based on having the `DataSource` property of the DGV set to a binding list of a class we'll call `DgvItem` that is minimally implemented as shown. The reason for making the `IsChecked` a binding property is in order to have the DataSource send a notification when its value changes (otherwise it only notifies when items are added or removed).

    class DgvItem : INotifyPropertyChanged
    {
        bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (!Equals(_IsChecked, value))
                {
                    _IsChecked = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Description { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

***
**Initialize**

The only thing left is to glue it all together in the `override` of the `Load` event in `MainForm`.

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        dataGridView.AllowUserToAddRows = false;

        // Set the DataSource of DGV to a binding list of 'DgvItem' instances.
        dataGridView.DataSource = DataSource;
        for (int i = 1; i <= 5; i++)
        {
            DataSource.Add(new DgvItem { Description = $"Item {i}" });
        }

        // Do a little column formatting
        dataGridView.Columns[nameof(DgvItem.IsChecked)].Width = 50;
        dataGridView.Columns[nameof(DgvItem.IsChecked)].HeaderText = string.Empty;
        dataGridView.Columns[nameof(DgvItem.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        // Ensure the row isn't stuck in edit mode when a checkbox changes
        dataGridView.CurrentCellDirtyStateChanged += (sender, e) =>
        {
            if(dataGridView.CurrentCell is DataGridViewCheckBoxCell)
            {
                dataGridView.EndEdit();
            }
        };

        // Get notified when a checkbox changes
        DataSource.ListChanged += (sender, e) =>
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    // Make changes to row styles when that happens.
                    refreshStyles();
                    break;
            }
        };

        // Init the styles
        refreshStyles();
    }

