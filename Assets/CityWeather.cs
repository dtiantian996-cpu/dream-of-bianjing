using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 城市天气
/// </summary>
public class CityWeather : MonoBehaviour
{
    public Image imgWeather;    //天气图片
    public Text textWeather;    //天气
    public Text textTemperature;//温度
    public Text textCity;       //城市

    // 把你的心知天气（或其他）API key 放这里
    private const string WeatherApiKey = "SLFIvbXeocQ0e_40C";

    void Start()
    {
        StartCoroutine(GetRuntimeWeather());
    }

    IEnumerator GetRuntimeWeather()
    {
        //1.获取本地公网IP
        UnityWebRequest wwwWebIp = UnityWebRequest.Get("https://api.ipify.org/");
        yield return wwwWebIp.SendWebRequest();

        // 新的错误检测方式：检查 result 是否为 Success
        if (wwwWebIp.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("获取公网IP失败: " + wwwWebIp.error);
            yield break;
        }

        string ipText = wwwWebIp.downloadHandler.text?.Trim();
        if (string.IsNullOrEmpty(ipText))
        {
            Debug.LogWarning("公网IP返回为空");
            yield break;
        }

        //2.根据IP查询城市（心知天气提供接口，需要申请key）***这里别忘记修改为你的密钥
        string urlQueryCity = $"https://api.seniverse.com/v3/location/search.json?key={WeatherApiKey}&q={UnityWebRequest.EscapeURL(ipText)}";
        UnityWebRequest wwwQueryCity = UnityWebRequest.Get(urlQueryCity);
        yield return wwwQueryCity.SendWebRequest();

        if (wwwQueryCity.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("查询城市失败: " + wwwQueryCity.error);
            yield break;
        }

        JObject cityData = null;
        try
        {
            cityData = JsonConvert.DeserializeObject<JObject>(wwwQueryCity.downloadHandler.text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("解析城市数据出错: " + ex.Message);
            yield break;
        }

        if (cityData == null || cityData["results"] == null || cityData["results"].HasValues == false)
        {
            Debug.LogWarning("未找到城市信息");
            yield break;
        }

        string cityId = cityData["results"][0]["id"]?.ToString();
        string cityName = cityData["results"][0]["name"]?.ToString();
        textCity.text = string.IsNullOrEmpty(cityName) ? "未知城市" : cityName;

        if (string.IsNullOrEmpty(cityId))
        {
            Debug.LogWarning("cityId 为空，无法查询天气");
            yield break;
        }

        //3.根据城市查询天气（心知天气提供接口，需要申请key）
        // 注意：使用 cityId（或名字）作为 location 参数，key 用你的真实 key 替换
        string urlWeather = string.Format(
            "https://api.seniverse.com/v3/weather/now.json?key={WeatherApiKey}&location=jinan&language=zh-Hans&unit=c",
            WeatherApiKey, UnityWebRequest.EscapeURL(cityId)
        );

        UnityWebRequest wwwWeather = UnityWebRequest.Get(urlWeather);
        yield return wwwWeather.SendWebRequest();

        if (wwwWeather.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("获取天气失败: " + wwwWeather.error);
            yield break;
        }

        //4.解析天气
        try
        {
            JObject weatherData = JsonConvert.DeserializeObject<JObject>(wwwWeather.downloadHandler.text);
            string spriteName = string.Format("Weather/{0}@2x", weatherData["results"][0]["now"]["code"].ToString());

            //天气文字
            textWeather.text = weatherData["results"][0]["now"]["text"].ToString();

            ChangeWeather.GetInstance().SetWeather(textWeather.text);

            //图片，可以在心知天气上下载
            imgWeather.sprite = Resources.Load<Sprite>(spriteName);
            Debug.Log(spriteName);

            //温度
            textTemperature.text = string.Format("{0} °C", weatherData["results"][0]["now"]["temperature"].ToString());
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}
