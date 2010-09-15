using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace UnRarIt.UI
{
    class LRUComboBox : ComboBox
    {
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (SelectedIndex > 0)
            {
                Add(Items[SelectedIndex] as string);
            }
        }

        public void Add(string value) {
            Items.Insert(0, value);
            for (int i = Items.Count - 1; i > 0; --i)
            {
                if (Items[i].ToString().ToLower() == value.ToLower())
                {
                    Items.RemoveAt(i);
                }
            }
            while (Items.Count > 10)
            {
                Items.RemoveAt(Items.Count - 1);
            }
            base.SelectedIndex = 0;
        }
        public void AddRange(object[] values)
        {
            Items.AddRange(values);
            if (Items.Count != 0)
            {
                base.SelectedIndex = 0;
            }
        }
    }
}
