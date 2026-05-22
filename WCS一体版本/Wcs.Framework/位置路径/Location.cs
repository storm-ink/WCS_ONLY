using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备位置<br />
    /// 系统内所有路径都是通过位置来进行连通
    /// </summary>
    public abstract class Location
    {
        Location[] _synonymous=new Location[0];

        #region Properities
        /// <summary>
        /// 位置所属的设备
        /// </summary>
        public Device Device { get; private set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        public virtual String UserCode { get; private set; }
        /// <summary>
        /// 该位置在设备中的编码形式
        /// </summary>
        public virtual String DeviceCode { get; private set; }
        /// <summary>
        /// 统一编号
        /// </summary>
        public virtual String UnifiedCode { get; private set; }

        /// <summary>
        /// 同义位置对象集合
        /// 和此对象表示的位置一样，但表述不一样的对象<br />
        /// 如00-101-001（输送线 1 号货位） 和 01-000-001（堆垛机 1 排 0 列 1层）<br />
        /// 该属性永远不会返回NULL
        /// </summary>
        public Location[] Synonymous
        {
            get
            {
                return _synonymous;
            }
        }
        #endregion

        public Location(String deviceCode, String userCode, Device device,string unifiedCode = null)
        {
            this.Device = device;
            this.DeviceCode = deviceCode;
            this.UserCode = userCode;
            if (unifiedCode != null)
                this.UnifiedCode = unifiedCode;
            else
                this.UnifiedCode = deviceCode;
        }

        public Location[] AddSynonymous(params Location[] synonymous)
        {
            if (synonymous == null)
            {
                throw new ArgumentNullException("synonymous");
            }

            if (synonymous.Any(x => x == this))
            {
                throw new InvalidOperationException(string.Format("尝试向 {0} 添加同义位置对象时失败。无法将自身添加到同义位置。", this));
            }

            //var invalidLoc = synonymous
            //    .Where(x => x.Synonymous.Length > 0);
            //if (invalidLoc.Count() > 0)
            //{
            //    throw new InvalidOperationException(string.Format("尝试向 {0} 添加同义位置对象时失败。无效的参数：{1} \n不支持嵌套同义位置对象。也就是说，你不能将设置了 Synonymous 属性值的 Location 对象添加到另一个对象的 Synonymous 属性值中。", this,string.Join(",", invalidLoc.Select(x => x.ToString()).ToArray())));
            //}

            //将子级也进行互连
            List<Location> allsynonymous = new List<Location>();
            foreach (var item in synonymous)
            {
                allsynonymous = getChildrenSynonymous(item, allsynonymous);
            }

            allsynonymous = allsynonymous.Distinct().ToList();

            _synonymous = _synonymous
                .Concat(allsynonymous)
                .Where(x=>x!=this)
                .GroupBy(x=>x.ToConvertibleCode())
                .Select(x=>x.First())
                .ToArray();

            return _synonymous;
        }

        List<Location> getChildrenSynonymous(Location loc, List<Location> allsynonymous)
        {
            allsynonymous.Add(loc);
            foreach (var item in loc.Synonymous)
            {
                if (allsynonymous.Any(x => x == item))
                {
                    continue;
                }

                getChildrenSynonymous(item, allsynonymous);
            }

            return allsynonymous.Distinct().ToList();
        }

        #region Overrides
        public override int GetHashCode()
        {
            return this.ToConvertibleCode().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var loc = obj as Location;
            if (loc == null)
            {
                return false;
            }

            if (loc== this)
            {
                return true;
            }

            if (string.Equals(this.ToConvertibleCode(), loc.ToConvertibleCode(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            //如果没有同义位置，则无需再对比
            if (loc.Synonymous.Length == 0)
            {
                return false;
            }

            //修改版（原理：两个同义位置必定包含在对方的 Synonymous 集合当中）
            if (loc.Synonymous.Any(x => x == this)
                || this.Synonymous.Any(x => x == loc))
            {
                return true;
            }


            if (loc is ILocationWildcard)
            {
                var locs = ((ILocationWildcard)this).GetMatchedLocations();
                if (locs.Any(x => x == loc))
                {
                    return true;
                }
                else
                {
                    return locs.Any(x => x.Equals(this));
                }
            }

            if (this is ILocationWildcard)
            {
                var locs = ((ILocationWildcard)this).GetMatchedLocations();
                if (locs.Any(x => x == loc))
                {
                    return true;
                }
                else
                {
                    return locs.Any(x => x.Equals(loc));
                }
            }

            return false;


            //老版
            //if (loc is ILocationWildcard)
            //{
            //    var locs = ((ILocationWildcard)this).GetMatchedLocations();
            //    if (locs.Any(x => x == loc))
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return locs.Any(x => x.Equals(this));
            //    }
            //    //return this.Device.Equals(loc.Device)
            //    //    || this.Synonymous.Any(x => x.Device.Equals(loc.Device))
            //    //    || loc.Synonymous.Any(x => x.Device.Equals(this.Device))
            //    //    || loc.Equals(this);
            //}

            //if (this is ILocationWildcard)
            //{
            //    var locs=((ILocationWildcard)this).GetMatchedLocations();
            //    if (locs.Any(x => x == loc))
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return locs.Any(x => x.Equals(loc));
            //    }
            //    //return this.Device.Equals(loc.Device)
            //    //    || this.Synonymous.Any(l => l.Equals(loc))
            //    //    || loc.Synonymous.Any(l => l.Equals(this));

            //}

            //return this.Synonymous.Any(x => x == loc)
            //    || loc.Synonymous.Any(x => x==this);

            //return base.Equals(obj)
            //    || this.Synonymous.Any(x => ReferenceEquals(loc, x))
            //    || loc.Synonymous.Any(x => ReferenceEquals(this, x));
        }

        public override string ToString()
        {
            return this.UserCode;
        }
        #endregion

        /// <summary>
        /// 转换为系统可识别（可二次转换的）编码值
        /// </summary>
        /// <returns>字符串，格式为：位置在设备中的编码形式@设备名称，如：01001001@c001</returns>
        public String ToConvertibleCode()
        {
            return String.Format("{0}@{1}", this.DeviceCode, this.Device.Name);
        }
        /// <summary>
        /// 将当前位置对象转换为参于任务路径拆分的位置，可视具体情况重写该方法。
        /// <para>默认情况下，一个位置类型是可以直接参与任务中的路径查询和拆分的，但是在某些时候并不适用，
        /// 可能需要将一个不存在的位置对象转换为一个已存在的位置对象参于任务路径计算,如双伸叉堆垛机中的TwoForksCraneTaskLocation位置类型。
        /// TwoForksCraneTaskLocation其实是一个虚拟的不存在于系统中的位置对象，它是由两个货格位置组合而来。如果直接使用它参于路径计算，将得不到任何想要的结果。</para>
        /// </summary>
        /// <returns></returns>
        public virtual Location ToTaskableLocation()
        {
            return this;
        }
    }
}
