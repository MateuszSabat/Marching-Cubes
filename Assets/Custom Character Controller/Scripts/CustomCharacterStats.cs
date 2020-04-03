using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCharacter
{
    public class CustomCharacterStats : MonoBehaviour
    {
        public CustomCharacter character;
        #region health
        public BarStat _health;
        public float health
        {
            get
            {
                return _health.value;
            }
            set
            {
                _health.value = value;
            }
        }
        public void TakeDamage(float damage)
        {
            health -= damage;
        }
        #endregion
        #region stamina
        public BarStat _stamina;
        public float stamina
        {
            get
            {
               return _stamina.value;
            }
            set
            {
                _stamina.value = value;
            }
        }

        public float staminaDelay;

        public float staminaGain;

        public float runStaminaDrop = 5f;

        public bool run;

        IEnumerator RegenerateStamina()
        {
            yield return new WaitForSeconds(staminaDelay);
            while (true)
            {
                if (stamina == _stamina.max)
                    yield break;
                stamina += Time.deltaTime * staminaGain;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator RunStaminaDrop()
        {
            if(stamina == _stamina.max)
                yield return new WaitForSeconds(staminaDelay);
            while (true)
            {
                if (stamina == _stamina.min)
                {
                    character.RunOutOffStaminaCall();
                    yield break;
                }
                if(character.tendToMove)
                    stamina -= Time.deltaTime * runStaminaDrop;
                yield return new WaitForEndOfFrame();
            }
        }

        public bool CanUseStamina(float sDrop)
        {
            stamina -= sDrop;

            if (!run)
            {
                StopCoroutine("RegenerateStamina");
                StartCoroutine("RegenerateStamina");
            }

            return stamina > _stamina.min;
        }

        public void StartRun()
        {
            if (!run)
            {
                run = true;
                StopCoroutine("RegenerateStamina");
                StartCoroutine("RunStaminaDrop");
            }
        }
        public void StopRun()
        {
            run = false;
            StopCoroutine("RunStaminaDrop");
            StartCoroutine("RegenerateStamina");
        }
        #endregion
    }

    public class Stat
    {

        public string name;
        public float statValue;        

        public Stat()
        {
            name = "stat";
            statValue = 0;
        }
        public Stat(string _name, float _value)
        {
            name = _name;
            statValue = _value;
        }
    }

    public class BarStat : Stat
    {
        public float min;
        public float max;
        public float value
        {
            set
            {
                statValue = Mathf.Clamp(value, min, max);
            }
            get
            {
                return statValue;
            }
        }

        public float percent
        {
            get
            {
                return value / (max - min);
            }
        }

        public BarStat()
        {
            name = "bar stat";
            min = 0;
            max = 100;
            value = 100;
        }
        public BarStat(string _name, float _min, float _max)
        {
            name = _name;
            min = _min;
            max = _max;
            value = _max;
        }
        public BarStat(string _name, float _min, float _max, float _value)
        {
            name = _name;
            min = _min;
            max = _max;
            value = _value;
        }
    }
}
