#include "FRC_CAN_Reader_Native.h"
#include "CANControllerDetector.h"

extern "C"
{
    struct CAN_Device *FRC_CAN_Reader_Native_EnumerateDevices(int *length)
    {
        auto cppDevices = can::CANControllerDetector::EnumerateDevices();
        size_t allocLength = sizeof(CAN_Device) * cppDevices.size();
        for (auto &&device : cppDevices)
        {
            length += device.name.size() + device.deviceId.size() + 2;
        }

        CAN_Device *devices = reinterpret_cast<CAN_Device *>(malloc(allocLength));
        if (devices == nullptr)
        {
            *length = 0;
            return nullptr;
        }
        *length = cppDevices.size();
        char *stringBase = reinterpret_cast<char *>(devices + cppDevices.size());
        CAN_Device *baseDevices = devices;
        for (auto &&device : cppDevices)
        {
            devices->name = stringBase;
            strcpy(stringBase, device.name.c_str());
            stringBase += device.name.size() + 1;
            devices->deviceId = stringBase;
            strcpy(stringBase, device.deviceId.c_str());
            stringBase += device.deviceId.size() + 1;
            devices++;
        }
        return baseDevices;
    }

    void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device *devices, int length)
    {
        free(devices);
    }

    CANDeviceHandle FRC_CAN_Reader_Native_Create(const struct CAN_Device *device)
    {
        can::CANDevice cppDevice;
        cppDevice.name = device->name;
        cppDevice.deviceId = device->deviceId;
        auto controller = can::CANControllerDetector::CreateController(cppDevice);
        return controller.release();
    }

    void FRC_CAN_Reader_Native_Free(CANDeviceHandle handle)
    {
        delete reinterpret_cast<CANController *>(handle);
    }

    WPI_EventHandle FRC_CAN_Reader_Native_GetEventHandle(CANDeviceHandle handle)
    {
        return reinterpret_cast<CANController *>(handle)->getEventHandle().GetHandle();
    }

    void FRC_CAN_Reader_Native_ReadMessage(CANDeviceHandle handle, struct CANData *data)
    {
        auto msg = reinterpret_cast<CANController *>(handle)->getData();
        *data = msg;
    }
}
