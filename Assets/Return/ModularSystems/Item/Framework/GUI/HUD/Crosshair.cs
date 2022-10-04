using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Crosshair : MonoBehaviour
{
    [SerializeField]
    private float m_CrosshairAperture = 192;

    [SerializeField]
    private RectTransform m_Up;

    [SerializeField]
    private RectTransform m_Down;

    [SerializeField]
    private RectTransform m_Right;

    [SerializeField]
    private RectTransform m_Left;

    [SerializeField]
    private RectTransform m_Center;

    public void SetActive(bool active)
    {
        if (m_Up)
            m_Up.gameObject.SetActive(active);

        if (m_Down)
            m_Down.gameObject.SetActive(active);

        if (m_Right)
            m_Right.gameObject.SetActive(active);

        if (m_Left)
            m_Left.gameObject.SetActive(active);

        if (m_Center)
            m_Center.gameObject.SetActive(active);
    }

    public void Move(float? accuracy)
    {
        float fixedAcccuracy = accuracy ?? 0;

        if (m_Up)
            m_Up.localPosition = new Vector3(0, m_CrosshairAperture * (1 - fixedAcccuracy));

        if (m_Down)
            m_Down.localPosition = new Vector3(0, -m_CrosshairAperture * (1 - fixedAcccuracy));

        if (m_Right)
            m_Right.localPosition = new Vector3(m_CrosshairAperture * (1 - fixedAcccuracy), 0);

        if (m_Left)
            m_Left.localPosition = new Vector3(-m_CrosshairAperture * (1 - fixedAcccuracy), 0);
    }
}

