using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Wcs.MethodTrack
{
    public class MethodDescriptor
    {
        public MethodDescriptor(String name)
            : this(name, new MethodDescriptor[0])
        {
        }

        public MethodDescriptor(String name, params MethodDescriptor[] parent)
        {
            this.ParentNodes = new List<MethodDescriptor>();
            this.ChildNodes = new List<MethodDescriptor>();
            this.Name = name;
            this.ParentNodes.AddRange(parent);
        }

        public String Name { get; set; }
        public String Description { get; set; }
        public List<MethodDescriptor> ParentNodes { get; set; }
        public List<MethodDescriptor> ChildNodes { get; set; }

        public Boolean AccessResult { get; set; }
        public DateTime? AccessAt { get; set; }
        public String AccessDescription { get; set; }

        public virtual void ClearAccess()
        {
            AccessResult = false;
            AccessAt = null;
            AccessDescription = null;
        }

        public void Access(Boolean result)
        {
            Access(result, null);
        }

        public void Access(Boolean result, String description)
        {
            AccessResult = result;
            AccessDescription = description;
            AccessAt = DateTime.Now;
        }

        public MethodDescriptor AddChildren(params MethodDescriptor[] childNodes)
        {
            var invaldNodes = childNodes.Intersect(ChildNodes);
            foreach (var item in childNodes.Except(invaldNodes))
            {
                item.AddParent(this);
                ChildNodes.Add(item);
            }

            return childNodes.First();
        }

        public MethodDescriptor AddParent(params MethodDescriptor[] parentNodes)
        {
            var invaldNodes = parentNodes.Intersect(ParentNodes);
            ParentNodes.AddRange(parentNodes.Except(invaldNodes));

            return parentNodes.First();
        }

        public List<MethodDescriptor> MethodDescriptors
        {
            get
            {
                List<MethodDescriptor> result = new List<MethodDescriptor>();
                result.Add(this);
                foreach (var item in ChildNodes)
                {
                    result.Add(item);
                    result.AddRange(item.MethodDescriptors);
                }

                return result.Distinct().ToList();
            }
        }

        public virtual Boolean TestHit(int x, int y)
        {
            var p = _位置;
            if ((x >= p.X && x <= p.X + _对象大小)
                && (y >= p.Y) && y <= (p.Y + _对象大小))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Int32 _对象大小 = 10;
        Int32 _与父级间隔 = 60;
        Int32 _同级间隔 = 60;
        PointF _位置
        {
            get
            {
                if (ParentNodes.Count > 0)
                {
                    var parent = ParentNodes.OrderByDescending(x => x.ChildNodes.Count).First();
                    if (parent.ChildNodes.Count == 1)
                    {

                        var x = ParentNodes.First()._位置.X;
                        var y = ParentNodes.First()._位置.Y + _与父级间隔;

                        return new PointF(x, y);
                    }
                    else
                    {
                        int widthTotals = parent.ChildNodes.Count * _对象大小 + (parent.ChildNodes.Count - 1) * _同级间隔;
                        float leftX = parent._位置.X - widthTotals / 2;
                        int index = parent.ChildNodes.IndexOf(this);
                        var x = leftX + _对象大小 * index + _同级间隔 * index;
                        var y = ParentNodes.First()._位置.Y + _与父级间隔;

                        return new PointF(x, y);
                    }
                }
                else
                {
                    return new PointF(30, 20);
                }
            }
        }

        public virtual void Paint(Graphics g)
        {
            Pen pen;
            if (AccessResult)
            {
                pen = new Pen(Brushes.Green, 1);
            }
            else
            {
                pen = new Pen(Brushes.Red, 1);
            }
            g.DrawArc(pen, new RectangleF(_位置.X, _位置.Y, _对象大小, _对象大小), 0, 360);
            g.DrawString(this.Name, new Font("宋体", 8f, FontStyle.Regular), pen.Brush, _位置.X + _对象大小, _位置.Y);

            foreach (var item in ParentNodes)
            {
                var p1 = new PointF(item._位置.X + _对象大小 / 2, item._位置.Y + _对象大小);
                var p2 = new PointF(_位置.X + _对象大小 / 2, _位置.Y);

                g.DrawLine(pen, p1, p2);
            }
        }
    }

}
