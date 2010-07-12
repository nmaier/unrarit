using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace UnRarIt.UI
{
    class ListView : System.Windows.Forms.ListView
    {
        public ListView()
            : base()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
        }
        protected override void OnNotifyMessage(Message m)
        {
            switch (m.Msg)
            {
                case 0x14:
                    return;

                default:
                    base.OnNotifyMessage(m);
                    return;
            }
        }
    }
}
