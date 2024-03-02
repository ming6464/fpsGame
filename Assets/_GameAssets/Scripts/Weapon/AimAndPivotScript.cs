using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimAndPivotScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform _weaponHolder;

    [Header("Aim")]
    [SerializeField]
    private MultiAimConstraint _headAim;

    [SerializeField]
    private MultiAimConstraint _weaponAim;

    [SerializeField]
    private float _smoothRotateAim;

    [Space(10)]
    [SerializeField]
    private Transform _pivotFireWeight0;

    private float m_weightValue;
    private Transform m_mainCamera;
    private Transform m_pivotFire;
    public Transform PivotRay => m_pivotFire;

    private void Awake()
    {
        m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }


    private void Update()
    {
        if (m_weightValue == 0)
        {
            _weaponHolder.localRotation = Quaternion.Lerp(_weaponHolder.localRotation, Quaternion.Euler(Vector3.zero),
                _smoothRotateAim * Time.deltaTime);
        }
    }

    public void SetUpAim(float value)
    {
        m_weightValue = value;
        if (_headAim)
        {
            _headAim.weight = value;
        }

        if (_weaponAim)
        {
            _weaponAim.weight = value;
        }

        if (value == 0)
        {
            m_pivotFire = _pivotFireWeight0;
        }
        else
        {
            m_pivotFire = m_mainCamera;
        }
    }
}