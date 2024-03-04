using System.Collections.Generic;
using Cinemachine;

public class CameraSwitcher
{
    private static CinemachineVirtualCamera m_activeCamera = null;
    private static List<CinemachineVirtualCamera> m_cameras = new();

    public static bool IsActiveCamera(CinemachineVirtualCamera cam)
    {
        return cam && m_activeCamera == cam;
    }

    public static bool RegisterCamera(CinemachineVirtualCamera cam)
    {
        if (!cam || m_cameras.Contains(cam))
        {
            return false;
        }

        m_cameras.Add(cam);
        return true;
    }

    public static void UnRegisterAllCam()
    {
        m_cameras = new List<CinemachineVirtualCamera>();
    }

    public static bool UnRegisterCamera(CinemachineVirtualCamera cam)
    {
        if (!cam || !m_cameras.Contains(cam))
        {
            return false;
        }

        m_cameras.Remove(cam);
        return true;
    }

    public static void SwitchNextCamera()
    {
        if (m_cameras.Count == 0)
        {
            return;
        }

        int indexNextCam = 0;
        if (m_activeCamera && m_cameras.Contains(m_activeCamera))
        {
            indexNextCam = m_cameras.IndexOf(m_activeCamera) + 1;
            if (m_cameras.Count == indexNextCam)
            {
                indexNextCam = 0;
            }
        }

        SwitchCamera(m_cameras[indexNextCam]);
    }

    public static void SwitchCamera(CinemachineVirtualCamera cam)
    {
        if (!m_cameras.Contains(cam))
        {
            return;
        }

        foreach (CinemachineVirtualCamera camloop in m_cameras)
        {
            camloop.Priority = 0;
        }

        cam.Priority = 10;
        m_activeCamera = cam;
    }
}