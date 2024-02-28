using System.Collections.Generic;
using Cinemachine;

public class CameraSwitcher
{
    public static CinemachineVirtualCamera ActiveCamera = null;
    private static List<CinemachineVirtualCamera> _cameras = new();

    public static bool IsActiveCamera(CinemachineVirtualCamera cam)
    {
        return cam && ActiveCamera == cam;
    }

    public static bool RegisterCamera(CinemachineVirtualCamera cam)
    {
        if (!cam || _cameras.Contains(cam))
        {
            return false;
        }

        _cameras.Add(cam);
        return true;
    }

    public static bool UnRegisterCamera(CinemachineVirtualCamera cam)
    {
        if (!cam || !_cameras.Contains(cam))
        {
            return false;
        }

        _cameras.Remove(cam);
        return true;
    }

    public static void SwitchNextCamera()
    {
        if (_cameras.Count == 0)
        {
            return;
        }

        int indexNextCam = 0;
        if (ActiveCamera && _cameras.Contains(ActiveCamera))
        {
            indexNextCam = _cameras.IndexOf(ActiveCamera) + 1;
            if (_cameras.Count == indexNextCam)
            {
                indexNextCam = 0;
            }
        }

        SwitchCamera(_cameras[indexNextCam]);
    }

    public static void SwitchCamera(CinemachineVirtualCamera cam)
    {
        if (!_cameras.Contains(cam))
        {
            return;
        }

        foreach (CinemachineVirtualCamera camloop in _cameras)
        {
            camloop.Priority = 0;
        }

        cam.Priority = 10;
        ActiveCamera = cam;
    }
}