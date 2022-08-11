using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Reflection;

namespace Std20.EasyArchitect.ApiHostBase
{
    /// <summary>
    /// ApiHostBase for .NET Standard 2.0
    /// </summary>
    [Route("api/[controller]/{dllName}/{nameSpace}/{className}/{methodName}/{*pathInfo}")]
    [Route("api/[controller]/{dllName}/{nameSpace}/{methodName}/{*pathInfo}")]
    [Route("api/[controller]/{dllName}/{methodName}/{*pathInfo}")]
    [Route("api/[controller]/{dllName}/{*pathInfo}")]
    [Route("api/[controller]/{*pathInfo}")]
    [ApiController]
    public class ApiHostBase: ControllerBase
    {
        /// <summary>
        /// 處理 Get 呼叫
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="nameSpace"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public ActionResult<object> Get(string dllName, string nameSpace, string className, string methodName)
        {
            if(string.IsNullOrEmpty(dllName) ||
                string.IsNullOrEmpty(nameSpace) ||
                string.IsNullOrEmpty(className) ||
                string.IsNullOrEmpty(methodName))
            {
                return GetJSONMessage("輸入的 Url 有誤！請確認！");
            }

            Assembly target = Assembly.Load($"{dllName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            if(target == null)
            {
                return GetJSONMessage($"找不到名稱為 {dllName} 的 DLL，請確認該 DLL 有存在在 bin 資料夾中！");
            }

            object result = null;
            Type targetType = target.GetType($"{nameSpace}.{className}");
            object targetIns = Activator.CreateInstance(targetType);
            var methodResult = targetType.GetMethods(
                BindingFlags.Default | 
                BindingFlags.Public | 
                BindingFlags.Instance)
                .Where(c => c.Name.ToLower() == methodName.ToLower())
                .FirstOrDefault();

            if(methodResult == null)
            {
                return GetJSONMessage($"找不到名稱為 {dllName} 的方法名稱，請確認該 DLL 有存在該 public 的 {methodName} 名稱！");
            }

            var queryString = Request.Query;
            
            if(queryString.Count() > 0)
            {
                ParameterInfo[] psInfo = methodResult.GetParameters();
                if(psInfo.Count() > 0)
                {
                    Type psType = psInfo[0].ParameterType;
                    object paramIns = Activator.CreateInstance(psType);
                    PropertyInfo[] properties = psType.GetProperties();

                    foreach (var q in queryString)
                    {
                        string keyName = q.Key;
                        
                        queryString.TryGetValue(q.Key, out var keyValue);
                        
                        var paramInsResult = properties
                            .Where(c => c.Name.ToLower() == keyName.ToLower())
                            .FirstOrDefault();

                        if(paramInsResult != null)
                        {
                            if(paramInsResult.PropertyType == typeof(int))
                            {
                                paramInsResult.SetValue(paramIns, Convert.ChangeType(keyValue.ToString(), paramInsResult.PropertyType));
                            }
                        }
                    }
                    result = methodResult.Invoke(targetIns, new object[] { paramIns });
                }
            }
            else
            {
                result = methodResult.Invoke(targetIns, null);
            }

            return result;
        }

        private ActionResult<object> GetJSONMessage(string message)
        {
            return new string[] { message }
                .Select(c => new
                {
                    Err = c
                }).ToList();
        }
    }
}
