#include "FRC_CAN_Reader_Native.h"
#include "Windows/WindowsCANController.h"

static WindowsCANController* canController;

extern "C" {
    struct CAN_Device* FRC_CAN_Reader_Native_EnumerateDevices(int* length) {
        auto cppDevices = WindowsCANController::getDevices();
        CAN_Device* devices = (CAN_Device*)malloc(sizeof(CAN_Device) * cppDevices.size());
        *length = cppDevices.size();
        for (size_t i = 0; i < cppDevices.size(); i++) {
            devices[i].name = (char*)malloc(cppDevices[i].first.size() * sizeof(char) + 1);
            strcpy(devices[i].name, cppDevices[i].first.c_str());
            devices[i].deviceId = (char*)malloc(cppDevices[i].second.size() * sizeof(char) + 1);
            strcpy(devices[i].deviceId, cppDevices[i].second.c_str());
        }
        return devices;
    }

    void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device* devices, int length) {
        for (int i = 0; i < length; i++) {
            free(devices[i].name);
            free(devices[i].deviceId);
        }
        free(devices);
    }

    void FRC_CAN_Reader_Native_Start() {
        canController = new WindowsCANController();
    }

    void FRC_CAN_Reader_Native_SetDevice(const char* deviceId) {
        canController->selectDevice(deviceId);
    }

    int FRC_CAN_Reader_Native_ReadMessage(struct CANData* data) {
        auto msg = canController->getData();
        if (!msg) {
            return false;
        }
        *data = *msg;
        return true;
    }

    void FRC_CAN_Reader_Native_ReleaseMessage() {
        canController->releaseData();
    }
}
