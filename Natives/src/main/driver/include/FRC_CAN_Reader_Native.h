#pragma once

#include <stdint.h>
#include <wpi/Synchronization.h>

struct CAN_Device {
    char* deviceId;
    char* name;
};

struct CANData {
  uint64_t timestamp;
  int32_t id;
  uint8_t length;
  uint8_t data[8];
};

typedef void* CANDeviceHandle;

extern "C" {

  struct CAN_Device* FRC_CAN_Reader_Native_EnumerateDevices(int* length);
  void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device* devices, int length);

  CANDeviceHandle FRC_CAN_Reader_Native_Create(const struct CAN_Device* device);
  void FRC_CAN_Reader_Native_Free(CANDeviceHandle handle);

  WPI_EventHandle FRC_CAN_Reader_Native_GetEventHandle(CANDeviceHandle handle);

  void FRC_CAN_Reader_Native_ReadMessage(CANDeviceHandle handle, struct CANData* data);
}
