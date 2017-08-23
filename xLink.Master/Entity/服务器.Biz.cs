﻿/*
 * XCoder v6.9.6298.42194
 * 作者：nnhy/X3
 * 时间：2017-03-31 23:16:06
 * 版权：版权所有 (C) 新生命开发团队 2002~2017
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NewLife.Data;
using NewLife.Web;
using XCode;

namespace xLink.Master.Entity
{
    /// <summary>服务器</summary>
    public partial class Server : Entity<Server>
    {
        #region 对象操作
            ﻿

        /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew"></param>
        public override void Valid(Boolean isNew)
        {
			// 如果没有脏数据，则不需要进行任何处理
			if (!HasDirty) return;

            // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
            //if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(_.Name, _.Name.DisplayName + "无效！");
            //if (!isNew && ID < 1) throw new ArgumentOutOfRangeException(_.ID, _.ID.DisplayName + "必须大于0！");

            // 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
            base.Valid(isNew);

            // 在新插入数据或者修改了指定字段时进行唯一性验证，CheckExist内部抛出参数异常
            //if (isNew || Dirtys[__.Name]) CheckExist(__.Name);
            
            if (!Dirtys[__.LastLoginIP]) LastLoginIP = WebHelper.UserHost;
            if (!Dirtys[__.RegisterIP]) RegisterIP = WebHelper.UserHost;
            if (isNew && !Dirtys[__.CreateTime]) CreateTime = DateTime.Now;
            if (!Dirtys[__.CreateIP]) CreateIP = WebHelper.UserHost;
            if (!Dirtys[__.UpdateTime]) UpdateTime = DateTime.Now;
            if (!Dirtys[__.UpdateIP]) UpdateIP = WebHelper.UserHost;
        }

        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    base.InitData();

        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    // Meta.Count是快速取得表记录数
        //    if (Meta.Count > 0) return;

        //    // 需要注意的是，如果该方法调用了其它实体类的首次数据库操作，目标实体类的数据初始化将会在同一个线程完成
        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化{0}[{1}]数据……", typeof(Server).Name, Meta.Table.DataTable.DisplayName);

        //    var entity = new Server();
        //    entity.Name = "abc";
        //    entity.Code = "abc";
        //    entity.Password = "abc";
        //    entity.DisplayName = "abc";
        //    entity.Location = "abc";
        //    entity.Enable = true;
        //    entity.Type = "abc";
        //    entity.Version = "abc";
        //    entity.InternalUri = "abc";
        //    entity.ExternalUri = "abc";
        //    entity.Online = true;
        //    entity.Logins = 0;
        //    entity.LastLogin = DateTime.Now;
        //    entity.LastLoginIP = "abc";
        //    entity.Registers = 0;
        //    entity.RegisterTime = DateTime.Now;
        //    entity.RegisterIP = "abc";
        //    entity.CreateUserID = 0;
        //    entity.CreateTime = DateTime.Now;
        //    entity.CreateIP = "abc";
        //    entity.UpdateUserID = 0;
        //    entity.UpdateTime = DateTime.Now;
        //    entity.UpdateIP = "abc";
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化{0}[{1}]数据！", typeof(Server).Name, Meta.Table.DataTable.DisplayName);
        //}


        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnInsert()
        //{
        //    return base.OnInsert();
        //}

        #endregion

        #region 扩展属性


        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static Server FindByID(Int32 id)
        {
            if (Meta.Count < 1000) return Meta.Cache.Entities.FirstOrDefault(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];
        }

        /// <summary>根据名称。登录用户名查找</summary>
        /// <param name="name">名称。登录用户名</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static Server FindByName(String name)
        {
            if (Meta.Count < 1000) return Meta.Cache.Entities.FirstOrDefault(e => e.Name == name);

            return Find(__.Name, name);
        }

        /// <summary>根据唯一编码查找</summary>
        /// <param name="code">唯一编码</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static Server FindByCode(String code)
        {
            if (Meta.Count < 1000) return Meta.Cache.Entities.FirstOrDefault(e => e.Code == code);

            return Find(__.Code, code);
        }
        #endregion

        #region 高级查询
        /// <summary>高级查询</summary>
        /// <param name="type"></param>
        /// <param name="enable"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IList<Server> Search(String type, Boolean? enable, DateTime start, DateTime end, String key, PageParameter param)
        {
            // WhereExpression重载&和|运算符，作为And和Or的替代
            // SearchWhereByKeys系列方法用于构建针对字符串字段的模糊搜索，第二个参数可指定要搜索的字段
            var exp = SearchWhereByKeys(key, null, null);

            if (!type.IsNullOrEmpty()) exp &= _.Type == type;
            if (enable != null) exp &= _.Enable == enable.Value;

            //exp &= _.CreateTime.Between(start, end);
            exp &= _.LastLogin.Between(start, end);

            return FindAll(exp, param);
        }
        #endregion

        #region 扩展操作
        #endregion

        #region 业务
        #endregion
    }
}