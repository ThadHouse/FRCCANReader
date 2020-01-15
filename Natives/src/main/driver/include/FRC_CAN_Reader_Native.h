#pragma once

#include <stdint.h>

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


extern "C" {
    struct CAN_Device* FRC_CAN_Reader_Native_EnumerateDevices(int* length);
    void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device* devices, int length);

    void FRC_CAN_Reader_Native_Start();
    void FRC_CAN_Reader_Native_SetDevice(const char* deviceId);

    int FRC_CAN_Reader_Native_ReadMessage(struct CANData* data);
    void FRC_CAN_Reader_Native_ReleaseMessage();
}
