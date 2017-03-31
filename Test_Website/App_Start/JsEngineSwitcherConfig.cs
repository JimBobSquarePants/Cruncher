using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace Test_Website 
{ 
    /// <summary>
    /// 
    /// </summary>
    public class JsEngineSwitcherConfig 
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engineSwitcher"></param>
        public static void Configure(JsEngineSwitcher engineSwitcher) 
        {
            engineSwitcher.EngineFactories.AddV8();
            engineSwitcher.DefaultEngineName = V8JsEngine.EngineName;
        }
    }
}