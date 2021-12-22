#include "FRC_CAN_Reader_Native.h"
#include "CANControllerDetector.h"
#include "wpi/MemAlloc.h"

extern "C"
{
    struct CAN_Device *FRC_CAN_Reader_Native_EnumerateDevices(int32_t *length)
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

    void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device *devices, int32_t length)
    {
        free(devices);
    }

    CANDeviceHandle FRC_CAN_Reader_Native_Create()
    {
        auto controller = can::CANControllerDetector::CreateController();
        return controller.release();
    }

    void FRC_CAN_Reader_Native_SetDevice(CANDeviceHandle handle, const char *deviceId)
    {
        reinterpret_cast<CANController *>(handle)->SelectDevice(deviceId);
    }

    void FRC_CAN_Reader_Native_Free(CANDeviceHandle handle)
    {
        delete reinterpret_cast<CANController *>(handle);
    }

    WPI_EventHandle FRC_CAN_Reader_Native_GetEventHandle(CANDeviceHandle handle)
    {
        return reinterpret_cast<CANController *>(handle)->GetEventHandle().GetHandle();
    }

    struct CANData *FRC_CAN_Reader_Native_ReadMessages(CANDeviceHandle handle, int32_t *dataLen)
    {
        auto msgs = reinterpret_cast<CANController *>(handle)->GetData();
        if (msgs.empty())
        {
            *dataLen = 0;
            return nullptr;
        }
        CANData *retData = reinterpret_cast<CANData *>(wpi::safe_malloc(sizeof(CANData) * msgs.size()));
        if (retData == nullptr)
        {
            *dataLen = 0;
            return nullptr;
        }
        std::copy(msgs.begin(), msgs.end(), retData);
        *dataLen = msgs.size();
        return retData;
    }

    void FRC_CAN_Reader_Native_FreeMessages(struct CANData *data, int32_t *dataLen)
    {
        free(data);
    }
}
