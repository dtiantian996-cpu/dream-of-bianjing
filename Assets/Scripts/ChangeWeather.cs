using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UniStorm;

//天气切换
public class ChangeWeather : MonoBehaviour
{

    [SerializeField] WeatherDataConfig weatherData; //天气数据 我们配置的文件拖入即可


    //单例
    static ChangeWeather instance;
    public static ChangeWeather GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
    }

    public void SetWeather(string weatherName)
    {
        //根据传过来的天气名称找到对应的插件天气下标
        int index = 0;
        for (int i = 0; i < weatherData.weatherData.Count; i++)
        {
            //名称相同 获取天气下标
            if (weatherData.weatherData[i].weatherName.Equals(weatherName))
            {
                index = weatherData.weatherData[i].uniStormWeatherIndex;
                break;
            }
        }

        //根据插件天气下标，获取天气
        WeatherType weather = UniStormSystem.Instance.AllWeatherTypes[index];
        Debug.Log(weather.WeatherTypeName);

        //UniStormManager.Instance.ChangeWeatherWithTransition(weather); //过度切换
        UniStormManager.Instance.ChangeWeatherInstantly(weather); //直接切换
    }
}