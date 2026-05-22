using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Wcs.MethodTrack
{

    public class MethodDescriptorTree : MethodDescriptor
    {
        public MethodDescriptorTree(String name)
            : base(name)
        {
        }

        public MethodDescriptor this[String name]
        {
            get
            {
                return AllMethodDescriptors.Single(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public List<MethodDescriptor> AllMethodDescriptors
        {
            get
            {
                List<MethodDescriptor> result = new List<MethodDescriptor>();
                result.Add(this);
                foreach (var item in ChildNodes)
                {
                    result.AddRange(item.MethodDescriptors);
                }

                return result.Distinct().ToList();
            }
        }

        public override void ClearAccess()
        {
            base.ClearAccess();
            foreach (var item in AllMethodDescriptors.Where(x => x != this))
            {
                item.ClearAccess();
            }
        }

        public override void Paint(Graphics g)
        {
            base.Paint(g);

            foreach (var item in AllMethodDescriptors.Where(x=>x!=this))
            {
                item.Paint(g);
            }
        }
    }
}
