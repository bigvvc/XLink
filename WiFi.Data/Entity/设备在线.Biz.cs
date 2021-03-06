﻿/*
 * XCoder v6.9.6298.42194
 * 作者：nnhy/X3
 * 时间：2017-03-31 22:14:32
 * 版权：版权所有 (C) 新生命开发团队 2002~2017
*/
using NewLife.Data;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using XCode;
using XCode.Membership;

namespace WiFi.Entity
{
    /// <summary>设备在线</summary>
    public partial class DeviceOnline : Entity<DeviceOnline>
    {
        #region 对象操作
        static DeviceOnline()
        {
            var df = Meta.Factory.AdditionalFields;
            df.Add(__.Total);

            Meta.Modules.Add<TimeModule>();
            Meta.Modules.Add<IPModule>();

            var sc = Meta.SingleCache;
            sc.FindSlaveKeyMethod = k => Find(_.SessionID == k);
            sc.GetSlaveKeyMethod = e => e.SessionID;
        }
        #endregion

        #region 扩展属性
        /// <summary>设备</summary>
        [XmlIgnore, ScriptIgnore]
        public Device Device => Extends.Get(nameof(Device), k => Device.FindByID(DeviceID));

        /// <summary>设备</summary>
        [Map(__.DeviceID)]
        public String DeviceName => Device + "";

        /// <summary>最后主机</summary>
        public Device Host => Extends.Get(nameof(Host), k => Device.FindByID(HostID));

        /// <summary>最后主机</summary>
        [Map(__.HostID)]
        public String HostName => Host + "";

        /// <summary>最后路由</summary>
        public Device Route => Extends.Get(nameof(Route), k => Device.FindByID(RouteID));

        /// <summary>最后路由</summary>
        [Map(__.RouteID)]
        public String RouteName => Route + "";
        #endregion

        #region 扩展查询
        /// <summary>根据会话查找</summary>
        /// <param name="deviceid">会话</param>
        /// <returns></returns>
        public static DeviceOnline FindByDeviceID(Int32 deviceid)
        {
            return Find(__.DeviceID, deviceid);
        }

        /// <summary>根据会话查找</summary>
        /// <param name="sessionid">会话</param>
        /// <returns></returns>
        public static DeviceOnline FindBySessionID(String sessionid)
        {
            //if (Meta.Count >= 1000)
            //return Find(__.SessionID, sessionid);
            //else // 实体缓存
            //    return Meta.Cache.Entities.FirstOrDefault(e => e.SessionID == sessionid);

            return Meta.SingleCache.GetItemWithSlaveKey(sessionid) as DeviceOnline;
        }

        /// <summary>根据会话查找</summary>
        /// <param name="sessionid">会话</param>
        /// <param name="cache">是否走缓存</param>
        /// <returns></returns>
        public static DeviceOnline FindBySessionID(String sessionid, Boolean cache)
        {
            if (!cache) return Find(_.SessionID == sessionid);

            //if (Meta.Count >= 1000)
            //return Find(__.SessionID, sessionid);
            //else // 实体缓存
            //    return Meta.Cache.Entities.FirstOrDefault(e => e.SessionID == sessionid);

            return Meta.SingleCache.GetItemWithSlaveKey(sessionid) as DeviceOnline;
        }
        #endregion

        #region 高级查询
        /// <summary>查询满足条件的记录集，分页、排序</summary>
        /// <param name="kind">类型</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="key">关键字</param>
        /// <param name="page">分页排序参数，同时返回满足条件的总记录数</param>
        /// <returns>实体集</returns>
        public static IList<DeviceOnline> Search(DeviceKinds kind, DateTime start, DateTime end, String key, PageParameter page)
        {
            // 修改DeviceID排序为名称
            //param = new PageParameter(param);
            if (page.Sort.EqualIgnoreCase(__.DeviceID)) page.Sort = __.Name;

            var list = Search(kind, start, end, key, page, false);
            // 如果结果为0，并且有key，则使用扩展查询，对内网外网地址进行模糊查询
            if (list.Count == 0 && !key.IsNullOrEmpty()) list = Search(kind, start, end, key, page, true);

            // 换回来，避免影响生成升序降序
            if (page.Sort.EqualIgnoreCase(__.Name)) page.Sort = __.DeviceID;

            return list;
        }

        private static IList<DeviceOnline> Search(DeviceKinds kind, DateTime start, DateTime end, String key, PageParameter page, Boolean ext)
        {
            var exp = new WhereExpression();

            if (kind > 0) exp &= _.Kind == kind;

            exp &= _.CreateTime.Between(start, end);

            if (!key.IsNullOrEmpty())
            {
                if (ext)
                    exp &= (_.Name.Contains(key) | _.SessionID.Contains(key));
                else
                    exp &= _.Name.StartsWith(key);
            }

            return FindAll(exp, page);
        }
        #endregion

        #region 扩展操作
        #endregion

        #region 业务
        /// <summary>根据编码查询或添加</summary>
        /// <param name="sessionid"></param>
        /// <returns></returns>
        public static DeviceOnline GetOrAdd(String sessionid) => GetOrAdd(sessionid, FindBySessionID, k => new DeviceOnline { SessionID = k });

        /// <summary>删除过期，指定过期时间</summary>
        /// <param name="secTimeout">超时时间，秒</param>
        /// <returns></returns>
        public static IList<DeviceOnline> ClearExpire(Int32 secTimeout)
        {
            // 10分钟不活跃将会被删除
            var exp = _.UpdateTime < DateTime.Now.AddSeconds(-secTimeout);
            var list = FindAll(exp, null, null, 0, 0);
            list.Delete();

            return list;
        }
        #endregion
    }
}