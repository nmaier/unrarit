using System;
using System.Windows.Forms;

namespace UnRarIt.UI
{
  internal class LRUComboBox : ComboBox
  {
    protected override void OnSelectedIndexChanged(EventArgs e)
    {
      if (SelectedIndex > 0) {
        Add(Items[SelectedIndex] as string);
      }
    }

    public void Add(string value)
    {
      Items.Insert(0, value);
      for (var i = Items.Count - 1; i > 0; --i) {
        if (Items[i].ToString().ToUpperInvariant() == value.ToUpperInvariant()) {
          Items.RemoveAt(i);
        }
      }
      while (Items.Count > 10) {
        Items.RemoveAt(Items.Count - 1);
      }
      base.SelectedIndex = 0;
    }
    public void AddRange(object[] values)
    {
      Items.AddRange(values);
      if (Items.Count != 0) {
        base.SelectedIndex = 0;
      }
    }
  }
}
