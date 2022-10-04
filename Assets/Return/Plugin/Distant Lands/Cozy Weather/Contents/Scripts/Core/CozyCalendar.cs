// Distant Lands 2021.





using UnityEngine;


namespace DistantLands.Cozy
{

    [ExecuteAlways]
    public class CozyCalendar : CozyModule
    {



        [Tooltip("Should this system reset the ticks when it loads or should it pull the current ticks from the scriptable object?")]
        public bool resetTicksOnStart = false;
        [Tooltip("The ticks that this system should start at when the scene is loaded.")]
        public float startTicks = 120;


        public string monthName;
        public string formattedTime;
        public bool continuousUpdate;



        #region Context Menu Items
        [ContextMenu("Dawn", false, 1000)]
        public void Dawn()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 75;

        }

        [ContextMenu("Morning", false, 1000)]
        public void Morning()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 90;

        }
        [ContextMenu("Noon", false, 1000)]
        public void Noon()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 180;

        }
        [ContextMenu("Afternoon", false, 1000)]
        public void Afternoon()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 240;

        }
        [ContextMenu("Evening", false, 1000)]
        public void Evening()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 260;

        }
        [ContextMenu("Twilight", false, 1000)]
        public void Twilight()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 285;

        }

        [ContextMenu("Night", false, 1000)]
        public void Night()
        {

            weatherSphere.perennialProfile.currentTicks = (weatherSphere.perennialProfile.ticksPerDay / 360) * 300;

        }

        [ContextMenu("Midnight", false, 1000)]
        public void Midnight()
        {

            weatherSphere.perennialProfile.currentTicks = 0;

        }

        [ContextMenu("Spring")]
        public void SetSeasonSpring()
        {

            weatherSphere.perennialProfile.currentDay = (weatherSphere.perennialProfile.daysPerYear / 4);

        }
        [ContextMenu("Summer")]
        public void SetSeasonSummer()
        {

            weatherSphere.perennialProfile.currentDay = (weatherSphere.perennialProfile.daysPerYear / 4) * 2;

        }
        [ContextMenu("Autumn")]
        public void SetSeasonAutumn()
        {

            weatherSphere.perennialProfile.currentDay = (weatherSphere.perennialProfile.daysPerYear / 4) * 3;

        }
        [ContextMenu("Winter")]
        public void SetSeasonWinter()
        {

            weatherSphere.perennialProfile.currentDay = (weatherSphere.perennialProfile.daysPerYear / 4) * 4;

        }

        #endregion



        void Awake()
        {

            if (!enabled)
                return;

            base.SetupModule(); 

            if (resetTicksOnStart)
                weatherSphere.perennialProfile.currentTicks = startTicks;


            if (weatherSphere.perennialProfile.realisticYear)
            {

                weatherSphere.perennialProfile.daysPerYear = weatherSphere.perennialProfile.RealisticDaysPerYear();


            }

        }

        // Update is called once per frame
        public void Update()
        {

            if (weatherSphere == null)
                if (CozyWeather.instance)
                    SetupModule();
                else
                {
                    Debug.LogError("Could not find an instance of COZY");
                    return;
                }

            ManageTime();

            monthName = MonthTitle(weatherSphere.perennialProfile.dayAndTime / weatherSphere.perennialProfile.daysPerYear);
            formattedTime = FormatTime(false);

            if (continuousUpdate)
            if (weatherSphere.perennialProfile.realisticYear)
            {

                    weatherSphere.perennialProfile.daysPerYear = weatherSphere.perennialProfile.RealisticDaysPerYear();


            }

            

        }


        public void ManageTime()
        {

            if (Application.isPlaying && !weatherSphere.perennialProfile.pauseTime)
                weatherSphere.perennialProfile.currentTicks += weatherSphere.perennialProfile.ModifiedTickSpeed() * Time.deltaTime;

            if (weatherSphere.perennialProfile.currentTicks > weatherSphere.perennialProfile.ticksPerDay)
            {
                weatherSphere.perennialProfile.currentTicks -= weatherSphere.perennialProfile.ticksPerDay;
                weatherSphere.perennialProfile.currentDay++;
            }

            if (weatherSphere.perennialProfile.currentTicks < 0)
            {
                weatherSphere.perennialProfile.currentTicks += weatherSphere.perennialProfile.ticksPerDay;
                weatherSphere.perennialProfile.currentDay--;
            }

            if (weatherSphere.perennialProfile.currentDay >= weatherSphere.perennialProfile.daysPerYear)
            {
                weatherSphere.perennialProfile.currentDay -= weatherSphere.perennialProfile.daysPerYear;
                weatherSphere.perennialProfile.currentYear++;
            }

            if (weatherSphere.perennialProfile.currentDay < 0)
            {
                weatherSphere.perennialProfile.currentDay += weatherSphere.perennialProfile.daysPerYear;
                weatherSphere.perennialProfile.currentYear--;
            }

            weatherSphere.perennialProfile.dayAndTime = weatherSphere.perennialProfile.currentDay + (weatherSphere.perennialProfile.currentTicks / weatherSphere.perennialProfile.ticksPerDay);

        }


        public string MonthTitle(float month)
        {

            if (weatherSphere.perennialProfile.realisticYear)
            {

                weatherSphere.perennialProfile.GetCurrentMonth(out string monthName, out int monthDay, out float monthPercentage);

                return monthName + " " + monthDay;


            }
            else
            {



                string monthName = "";
                string monthTimeName = "";

                float j = Mathf.Floor(month * 12);
                float monthLength = weatherSphere.perennialProfile.ticksPerDay / 12;
                float monthTime = weatherSphere.perennialProfile.dayAndTime - (j * monthLength);

                switch (j)
                {
                    case 0:
                        monthName = "January";
                        break;
                    case 1:
                        monthName = "February";
                        break;
                    case 2:
                        monthName = "March";
                        break;
                    case 3:
                        monthName = "April";
                        break;
                    case 4:
                        monthName = "May";
                        break;
                    case 5:
                        monthName = "June";
                        break;
                    case 6:
                        monthName = "July";
                        break;
                    case 7:
                        monthName = "August";
                        break;
                    case 8:
                        monthName = "September";
                        break;
                    case 9:
                        monthName = "October";
                        break;
                    case 10:
                        monthName = "November";
                        break;
                    case 11:
                        monthName = "December";
                        break;


                }


                if ((monthTime / monthLength) < 0.33f)
                    monthTimeName = "Early";
                else if ((monthTime / monthLength) > 0.66f)
                    monthTimeName = "Late";
                else
                    monthTimeName = "Mid";


                return monthTimeName + " " + monthName;
            }
        }

        public string FormatTime(bool militaryTime, float ticks)
        {

            float time = ticks / weatherSphere.perennialProfile.ticksPerDay;

            int minutes = Mathf.RoundToInt(time * 1440);
            int hours = (minutes - minutes % 60) / 60;
            minutes -= hours * 60;

            if (militaryTime)
                return "" + hours.ToString("D2") + ":" + minutes.ToString("D2");
            else if (hours == 0)
                return "" + 12 + ":" + minutes.ToString("D2") + "AM";
            else if (hours == 12)
                return "" + 12 + ":" + minutes.ToString("D2") + "PM";
            else if (hours > 12)
                return "" + (hours - 12) + ":" + minutes.ToString("D2") + "PM";
            else
                return "" + (hours) + ":" + minutes.ToString("D2") + "AM";

        }

        public string FormatTime(bool militaryTime)
        {

            float time = weatherSphere.perennialProfile.currentTicks / weatherSphere.perennialProfile.ticksPerDay;

            int minutes = Mathf.RoundToInt(time * 1440);
            int hours = (minutes - minutes % 60) / 60;
            minutes -= hours * 60;

            if (militaryTime)
                return "" + hours.ToString("D2") + ":" + minutes.ToString("D2");
            else if (hours == 0)
                return "" + 12 + ":" + minutes.ToString("D2") + "AM";
            else if (hours == 12)
                return "" + 12 + ":" + minutes.ToString("D2") + "PM";
            else if (hours > 12)
                return "" + (hours - 12) + ":" + minutes.ToString("D2") + "PM";
            else
                return "" + (hours) + ":" + minutes.ToString("D2") + "AM";

        }

        public float YearPercentage() { return weatherSphere.perennialProfile.dayAndTime / weatherSphere.perennialProfile.daysPerYear; }
        public float YearPercentage(float inTicks) { return (weatherSphere.perennialProfile.dayAndTime + (inTicks / weatherSphere.perennialProfile.ticksPerDay)) / weatherSphere.perennialProfile.daysPerYear; }

    }
}