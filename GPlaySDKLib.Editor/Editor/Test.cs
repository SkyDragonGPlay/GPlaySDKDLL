//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//using System.Text.RegularExpressions;

//namespace GPlay
//{

//    public class Test
//    {
//        [MenuItem("GPlay/ScriptingBackend")]
//        static void PrintScriptingBackend()
//        {
//            int result = 0;
//            PlayerSettings.GetPropertyOptionalInt("ScriptingBackend", ref result, BuildTargetGroup.Unknown);
//            Debug.Log("xxx adfasd  " + result);
//            //string dir = Application.dataPath + "/" + "xxx/Resources";
//            //string[] files = Directory.GetFiles(dir, "*.wav");

//            //for(int i=0; i<files.Length; ++i)
//            //{
//            //    string relativeFilePath = files[i].Replace(Application.dataPath + "/", "");
//            //    AssetDatabase.RenameAsset("assets/" + relativeFilePath, i.ToString());
//            //}
//            //AssetDatabase.Refresh();
//        }

//        static void TestPreloadCallbackInfoParse(string responseInfoStr)
//        {
//            Regex regex = new Regex(@"\{""resultCode"":(?<resultCode>[+\-\d]+), ""errorCode"":(?<errorCode>[+\-\d]+), ""groupName"":(?<groupName>[\s\S]*), ""percent"":(?<percent>[+\-\.\d]+), ""downloadSpeed"":(?<downloadSpeed>[+\-\.\d]+)\}");

//            Match match = regex.Match(responseInfoStr);
//            var responseInfo = new PreloadResponseInfo();
//            responseInfo.resultCode = (EActionResultCode1)int.Parse(match.Groups["resultCode"].Value);
//            responseInfo.errorCode = int.Parse(match.Groups["errorCode"].Value);
//            responseInfo.groupName = match.Groups["groupName"].Value;
//            responseInfo.percent = float.Parse(match.Groups["percent"].Value);
//            responseInfo.downloadSpeed = float.Parse(match.Groups["downloadSpeed"].Value);
//            Debug.Log(responseInfo);
//        }

//        public class PreloadResponseInfo
//        {
//            public EActionResultCode1 resultCode;
//            public int errorCode;
//            public string groupName;
//            public float percent;
//            public float downloadSpeed;
//            public override string ToString()
//            {
//                return string.Format("reaultCode:{0}  errorCode:{1} groupName:{2} percent:{3} downloadSpeed:{4}",
//                    resultCode, errorCode, groupName, percent, downloadSpeed);
//            }
//        }

//        public class JsonTest
//        {
//            public string str;
//            public int num;

//            public override string ToString()
//            {
//                return string.Format("str: {0}   num: {1}", str, num);
//            }
//        }

//        [MenuItem("GPlay/PrintJsonStr")]
//        static void PrintJsonStr()
//        {
//            var responseInfo = new PreloadResponseInfo() { resultCode = EActionResultCode1.GPLAY_INIT_SUCCESS, errorCode = 2, groupName = "abcdie", percent = 0.5f, downloadSpeed = 102.4f };

//            string responseInfoStr = string.Format(@"{{""resultCode"":{0}, ""errorCode"":{1}, ""groupName"":{2}, ""percent"":{3}, ""downloadSpeed"":{4}}}",
//                        10003, 30, "asdib", 0.5f, 293.4f);
//            Debug.Log("responseInfoStr: " + responseInfoStr);

//            TestPreloadCallbackInfoParse(responseInfoStr);
//        }

//    }

//    public enum EActionResultCode1
//    {
//        GPLAY_INIT_SUCCESS = 0,                 // succeeding in initing sdk
//        GPLAY_INIT_FAIL = 1,                    // failing to init sdk

//        USER_LOGIN_RESULT_SUCCESS = 10000,      // login success
//        USER_LOGIN_RESULT_FAIL = 10001,         // login failed
//        USER_LOGIN_RESULT_CANCEL = 10002,       // login canceled
//        USER_LOGOUT_RESULT_SUCCESS = 10003,     // logout success
//        USER_LOGOUT_RESULT_FAIL = 10004,        // logout failed
//        USER_REGISTER_RESULT_SUCCESS = 10005,   // regiister sucess
//        USER_REGISTER_RESULT_FAIL = 10006,      // regiister failed
//        USER_REGISTER_RESULT_CANCEL = 10007,    // regiister Cancel
//        USER_BIND_RESULT_SUCESS = 10008,        // bind sucess
//        USER_BIND_RESULT_CANCEL = 10009,        // bind Cancel
//        USER_BIND_RESULT_FAILED = 100010,       // bind failed
//        USER_RESULT_NETWROK_ERROR = 10011,      // network error
//        USER_RESULT_USEREXTENSION = 19999,      // extension code

//        PAY_RESULT_SUCCESS = 20000,             // pay success
//        PAY_RESULT_FAIL = 20001,                // pay fail
//        PAY_RESULT_CANCEL = 20002,              // pay cancel
//        PAY_RESULT_INVALID = 20003,             // incompleting info
//        PAY_RESULT_NETWORK_ERROR = 20004,       // network error
//        PAY_RESULT_NOW_PAYING = 20005,          // paying now
//        PAY_RESULT_PAYEXTENSION = 29999,        // extension code

//        SHARE_RESULT_SUCCESS = 30000,           // share success
//        SHARE_RESULT_FAIL = 30001,              // share failed
//        SHARE_RESULT_CANCEL = 30002,            // share canceled
//        SHARE_RESULT_NETWORK_ERROR = 30003,     // network error
//        SHARE_RESULT_SHAREREXTENSION = 39999,   // extension code

//        SHORTCUT_RESULT_SUCCESS = 40000,
//        SHORTCUT_RESULT_FAILED = 40001,

//        CAPTURE_SCREEN_SUCCESS = 41000,
//        CAPTURE_SCREEN_FAILED = 41001,

//        PRELOAD_RESULT_SUCCESS = 50000,
//        PRELOAD_RESULT_PROGRESS,
//        PRELOAD_RESULT_FAILED,

//        PRELOAD_ERROR_NETWORK = 60000,
//        PRELOAD_ERROR_VERIFY_FAILED,
//        PRELOAD_ERROR_NO_SPACE,
//        PRELOAD_ERROR_UNKNOWN,
//        PRELOAD_ERROR_NONE
//    }

//}
