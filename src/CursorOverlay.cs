using System;
using System.Drawing;
using System.Windows.Forms;

namespace Snapshot_tool.src
{
    public class CursorOverlay : Form
    {
        private int _w, _h;
        private System.Windows.Forms.Timer _timer;
        private Action _onCapture;
        private bool _wasMouseDown = false;
        private bool _isCursorHidden = false; //track cursor
        private Func<Rectangle> _getMainAppBounds;

        public CursorOverlay(int w, int h, Action onCapture, Func<Rectangle> getMainAppBounds)
        {
            _w = w; _h = h;
            _onCapture = onCapture;
            _getMainAppBounds = getMainAppBounds;

            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = Color.Magenta;
            TransparencyKey = Color.Magenta;
            Bounds = Screen.PrimaryScreen.Bounds;

            //hide cursor in pic mode
            Cursor.Hide();
            _isCursorHidden = true;

            _timer = new System.Windows.Forms.Timer { Interval = 15 };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //display cursor when closing
            if (_isCursorHidden)
            {
                Cursor.Show();
                _isCursorHidden = false;
            }
            base.OnFormClosing(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Invalidate();

            Point mousePos = Cursor.Position;
            Rectangle mainAppBounds = _getMainAppBounds();

            //cursor visbility logic
            bool isOverApp = mainAppBounds.Contains(mousePos);

            if (isOverApp)
            {
                if (_isCursorHidden) //show cursor when hovering over the app
                {
                    Cursor.Show();
                    _isCursorHidden = false;
                }
                _wasMouseDown = true;
                return;
            }
            else
            {
                //hide cursor in the box
                if (!_isCursorHidden)
                {
                    Cursor.Hide();
                    _isCursorHidden = true;
                }
            }

            //captuire logic
            bool isMouseDown = MouseButtons == MouseButtons.Left;
            if (isMouseDown && !_wasMouseDown)
            {
                _onCapture?.Invoke();
            }
            _wasMouseDown = isMouseDown;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //hide the red box in the app
            if (!_isCursorHidden) return;

            Point cur = PointToClient(Cursor.Position);
            using (Pen p = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(p, cur.X - _w / 2, cur.Y - _h / 2, _w, _h);

                int size = 10;
                e.Graphics.DrawLine(p, cur.X - size, cur.Y, cur.X + size, cur.Y);
                e.Graphics.DrawLine(p, cur.X, cur.Y - size, cur.X, cur.Y + size);
            }
        }

        //allow mouse clicks
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTCLIENT = 1;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                Rectangle appBounds = _getMainAppBounds();
                if (appBounds.Contains(Cursor.Position))
                {
                    m.Result = HTTRANSPARENT; //pass click through app
                }
                else
                {
                    m.Result = HTCLIENT; //use click for snapshot
                }
                return;
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000;
                return cp;
            }
        }
    }
}