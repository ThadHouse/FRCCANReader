#pragma once

#include <stdint.h>
#include <wpi/Synchronization.h>

struct CAN_Device
{
  char *deviceId;
  char *name;
};

struct CANData
{
  uint64_t timestamp;
  int32_t id;
  uint8_t length;
  uint8_t data[8];
};

typedef void *CANDeviceHandle;

extern "C"
{
  struct CAN_Device *FRC_CAN_Reader_Native_EnumerateDevices(int32_t *length);
  void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device *devices, int32_t length);

  CANDeviceHandle FRC_CAN_Reader_Native_Create();
  void FRC_CAN_Reader_Native_Free(CANDeviceHandle handle);
  void FRC_CAN_Reader_Native_SetDevice(CANDeviceHandle handle, const char* deviceId);

  WPI_EventHandle FRC_CAN_Reader_Native_GetEventHandle(CANDeviceHandle handle);

  struct CANData *FRC_CAN_Reader_Native_ReadMessages(CANDeviceHandle handle, int32_t *dataLen);
  void FRC_CAN_Reader_Native_FreeMessages(struct CANData *data, int32_t *dataLen);
}
