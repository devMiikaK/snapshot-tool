using System;
using System.Drawing;
using System.Windows.Forms;

namespace Snapshot_tool.src
{
    public class CursorOverlay : Form
    {
        private System.Windows.Forms.Timer _timer;
        private Action _onCapture;
        private bool _wasMouseDown = false;
        private bool _isCursorHidden = false; //track cursor
        private Func<Rectangle> _getMainAppBounds;

        //draws red box
        private VisualOverlay _visuals;

        public CursorOverlay(int w, int h, Action onCapture, Func<Rectangle> getMainAppBounds)
        {
            _onCapture = onCapture;
            _getMainAppBounds = getMainAppBounds;

            //make mouse invisible to eye
            this.Opacity = 0.01;
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            _visuals = new VisualOverlay(w, h);
            _visuals.Show(this);
            //hide cursor in pic mode
            Cursor.Hide();
            _isCursorHidden = true;

            _timer = new System.Windows.Forms.Timer { Interval = 15 };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        //keep visuals synced with the form
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (_visuals != null)
            {
                _visuals.Location = this.Location;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // hide the cursor after this form is closed.
            _timer.Stop();

            if (_isCursorHidden)
            {
                Cursor.Show();
                _isCursorHidden = false;
            }
            _visuals?.Close();
            base.OnFormClosing(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //do nothing when closing
            if (this.IsDisposed || _visuals.IsDisposed) return;

            _visuals.Invalidate(); //draw box

            Point mousePos = Cursor.Position;
            Rectangle mainAppBounds = _getMainAppBounds();

            //cursor visbility logic
            bool isOverApp = mainAppBounds.Contains(mousePos);

            //toggle box visibility
            _visuals.SetBoxVisible(!isOverApp);

            if (isOverApp)
            {
                if (_isCursorHidden)
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

        //allow mouse clicks
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                if (_getMainAppBounds().Contains(Cursor.Position))
                {
                    m.Result = (IntPtr)HTTRANSPARENT; //pass click through app
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private class VisualOverlay : Form
        {
            private int _w, _h;
            private bool _drawBox = true;

            public VisualOverlay(int w, int h)
            {
                _w = w; _h = h;
                this.FormBorderStyle = FormBorderStyle.None;
                this.TopMost = true;
                this.ShowInTaskbar = false;
                this.DoubleBuffered = true;
                this.BackColor = Color.Magenta;
                this.TransparencyKey = Color.Magenta;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }

            public void SetBoxVisible(bool visible)
            {
                _drawBox = visible;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (!_drawBox) return;

                Point cur = this.PointToClient(Cursor.Position);
                using (Pen p = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(p, cur.X - (_w / 2), cur.Y - (_h / 2), _w, _h);
                }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x80 | 0x20;
                    return cp;
                }
            }
        }
    }
}