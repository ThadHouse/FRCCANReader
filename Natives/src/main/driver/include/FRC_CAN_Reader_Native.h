#pragma once

#include <stdint.h>

typedef void* HALSIM_Candle_Handle;

struct HALSIM_Candle_Device {
    char* deviceId;
};

extern "C" {
HALSIM_Candle_Handle HALSIM_Candle_Enable(const char* name, int32_t baud);
void HALSIM_Candle_Clean(HALSIM_Candle_Handle handle);

struct HALSIM_Candle_Device* HALSIM_Candle_GetDevices(int32_t* count);
void HALSIM_Candle_FreeDevices(struct HALSIM_Candle_Device* devices, int32_t count);
}
