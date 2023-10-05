using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cropper
{
    public class ControlCrop
    {
        public Rectangle rect;
        public bool allowDeformingDuringMovement = false;
        private bool mIsClick = false;
        private bool mMove = false;
        private int oldX;
        private int oldY;
        private int sizeNodeRect = 8;
        private Bitmap mBmp = null;
        private PosSizableRect nodeSelected = PosSizableRect.None;
        private bool m_Visible = true;
        private Color m_CropColor;

        public Control m_Control { get; set; }
        public bool Visible
        {
            get { return m_Visible; }
            set
            {
                m_Visible = value;

                {
                    switch (value)
                    {
                        case true: m_CropColor = m_Control.ForeColor; break;
                        case false: m_CropColor = Color.Transparent; break;
                        default:
                            break;
                    }

                    m_Control.Invalidate();
                }
            }
        }

        private void Visible_Changed(object sender, EventArgs e)
        {
            this.Visible = m_Visible;
        }

        //public Color CropColor { get; set m_CropColor; } = Color.Black;
        public Color CropColor
        {
            get { return m_Control.ForeColor; }
            set
            {
                m_CropColor = value;
                m_Control.ForeColor = m_CropColor;
            }
        }

        //public static Boolean ShowDragHandles { get; set; }

        public ControlCrop()
        {

        }

        public ControlCrop(Control control, Boolean isVisible)
        {
            m_Control = control;
            //Set the default crop color 
            m_Control.ForeColor = Color.Black;
            mIsClick = false;
            //Make the crop area less than the full control
            rect = new Rectangle(30, 30, control.Width - 60, control.Height - 60);
        }

        private enum PosSizableRect
        {
            UpMiddle, //Top
            LeftMiddle, //Left
            LeftBottom,//BottomLeft
            LeftUp, //TopLeft
            RightUp, //TopRight
            RightMiddle, //Right
            RightBottom,//BottomRight
            BottomMiddle, //Bottom
            None
        };

        public void Draw(Graphics g)
        {
            g.DrawRectangle(new Pen(m_CropColor), rect);

            foreach (PosSizableRect pos in Enum.GetValues(typeof(PosSizableRect)))
            {
                g.DrawRectangle(new Pen(m_CropColor), GetRect(pos));
            }
        }

        //public void SetBitmapFile(string filename)
        //{
        //    this.mBmp = new Bitmap(filename);
        //}

        //public void SetBitmap(Bitmap bmp)
        //{
        //    this.mBmp = bmp;
        //}

        public void SetControl(Control p)
        {
            this.m_Control = p;
            m_Control.MouseDown += new MouseEventHandler(Control_MouseDown);
            m_Control.MouseUp += new MouseEventHandler(Control_MouseUp);
            m_Control.MouseMove += new MouseEventHandler(Control_MouseMove);
            m_Control.Paint += new PaintEventHandler(Control_Paint);
            m_Control.SizeChanged += new EventHandler(SizeChanged_Changed);
            m_Control.VisibleChanged += new EventHandler(Visible_Changed);
            m_Control.ForeColorChanged += new EventHandler(ForeColor_Changed);
        }

        private void ForeColor_Changed(object sender, EventArgs e)
        {
            CropColor = m_Control.ForeColor;
            m_Control.Invalidate();
        }

        private void SizeChanged_Changed(object sender, EventArgs e)
        {
            //Update rectangle and grab handles to reflect new size
            rect = new Rectangle(sizeNodeRect, sizeNodeRect, m_Control.Width - 20, m_Control.Height - 20);
        }

        private void Control_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Draw(e.Graphics);
            }
            catch (Exception exp)
            {
                System.Console.WriteLine(exp.Message);
            }
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            mIsClick = true;

            nodeSelected = PosSizableRect.None;
            nodeSelected = GetNodeSelectable(e.Location);

            if (rect.Contains(new Point(e.X, e.Y)))
            {
                mMove = true;
            }
            oldX = e.X;
            oldY = e.Y;
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            mIsClick = false;
            mMove = false;
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            ChangeCursor(e.Location);
            if (mIsClick == false)
            {
                return;
            }

            Rectangle backupRect = rect;

            switch (nodeSelected)
            {
                case PosSizableRect.LeftUp:
                    rect.X += e.X - oldX;
                    rect.Width -= e.X - oldX;
                    rect.Y += e.Y - oldY;
                    rect.Height -= e.Y - oldY;
                    break;
                case PosSizableRect.LeftMiddle:
                    rect.X += e.X - oldX;
                    rect.Width -= e.X - oldX;
                    break;
                case PosSizableRect.LeftBottom:
                    rect.Width -= e.X - oldX;
                    rect.X += e.X - oldX;
                    rect.Height += e.Y - oldY;
                    break;
                case PosSizableRect.BottomMiddle:
                    rect.Height += e.Y - oldY;
                    break;
                case PosSizableRect.RightUp:
                    rect.Width += e.X - oldX;
                    rect.Y += e.Y - oldY;
                    rect.Height -= e.Y - oldY;
                    break;
                case PosSizableRect.RightBottom:
                    rect.Width += e.X - oldX;
                    rect.Height += e.Y - oldY;
                    break;
                case PosSizableRect.RightMiddle:
                    rect.Width += e.X - oldX;
                    break;

                case PosSizableRect.UpMiddle:
                    rect.Y += e.Y - oldY;
                    rect.Height -= e.Y - oldY;
                    break;

                default:
                    if (mMove)
                    {
                        rect.X = rect.X + e.X - oldX;
                        rect.Y = rect.Y + e.Y - oldY;
                    }
                    break;
            }
            oldX = e.X;
            oldY = e.Y;

            if (rect.Width < 5 || rect.Height < 5)
            {
                rect = backupRect;
            }

            TestIfRectInsideArea();

            m_Control.Invalidate();
        }

        private void TestIfRectInsideArea()
        {
            // Test if rectangle still inside the area.
            if (rect.X < 0) rect.X = 0;
            if (rect.Y < 0) rect.Y = 0;
            if (rect.Width <= 0) rect.Width = 1;
            if (rect.Height <= 0) rect.Height = 1;

            if (rect.X + rect.Width > m_Control.Width)
            {
                rect.Width = m_Control.Width - rect.X - 1; // -1 to be still show 
                if (allowDeformingDuringMovement == false)
                {
                    mIsClick = false;
                }
            }
            if (rect.Y + rect.Height > m_Control.Height)
            {
                rect.Height = m_Control.Height - rect.Y - 1;// -1 to be still show 
                if (allowDeformingDuringMovement == false)
                {
                    mIsClick = false;
                }
            }
        }

        private Rectangle CreateRectSizableNode(int x, int y)
        {
            return new Rectangle(x - sizeNodeRect / 2, y - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
        }

        private Rectangle GetRect(PosSizableRect p)
        {
            switch (p)
            {
                case PosSizableRect.LeftUp:
                    return CreateRectSizableNode(rect.X, rect.Y);

                case PosSizableRect.LeftMiddle:
                    return CreateRectSizableNode(rect.X, rect.Y + +rect.Height / 2);

                case PosSizableRect.LeftBottom:
                    return CreateRectSizableNode(rect.X, rect.Y + rect.Height);

                case PosSizableRect.BottomMiddle:
                    return CreateRectSizableNode(rect.X + rect.Width / 2, rect.Y + rect.Height);

                case PosSizableRect.RightUp:
                    return CreateRectSizableNode(rect.X + rect.Width, rect.Y);

                case PosSizableRect.RightBottom:
                    return CreateRectSizableNode(rect.X + rect.Width, rect.Y + rect.Height);

                case PosSizableRect.RightMiddle:
                    return CreateRectSizableNode(rect.X + rect.Width, rect.Y + rect.Height / 2);

                case PosSizableRect.UpMiddle:
                    return CreateRectSizableNode(rect.X + rect.Width / 2, rect.Y);
                default:
                    return new Rectangle();
            }
        }

        private PosSizableRect GetNodeSelectable(Point p)
        {
            foreach (PosSizableRect r in Enum.GetValues(typeof(PosSizableRect)))
            {
                if (GetRect(r).Contains(p))
                {
                    return r;
                }
            }
            return PosSizableRect.None;
        }

        private void ChangeCursor(Point p)
        {
            m_Control.Cursor = GetCursor(GetNodeSelectable(p));
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Cursor GetCursor(PosSizableRect p)
        {
            switch (p)
            {
                case PosSizableRect.LeftUp:
                    return Cursors.SizeNWSE;

                case PosSizableRect.LeftMiddle:
                    return Cursors.SizeWE;

                case PosSizableRect.LeftBottom:
                    return Cursors.SizeNESW;

                case PosSizableRect.BottomMiddle:
                    return Cursors.SizeNS;

                case PosSizableRect.RightUp:
                    return Cursors.SizeNESW;

                case PosSizableRect.RightBottom:
                    return Cursors.SizeNWSE;

                case PosSizableRect.RightMiddle:
                    return Cursors.SizeWE;

                case PosSizableRect.UpMiddle:
                    return Cursors.SizeNS;
                default:
                    return Cursors.Default;
            }
        }


    }
}
