#include "CANControllerDetector.h"

using namespace can;

std::vector<CANDevice> CANControllerDetector::EnumerateDevices() {
    return {};
}

std::unique_ptr<CANController> CANControllerDetector::CreateController(const CANDevice& device) {
    return nullptr;
}