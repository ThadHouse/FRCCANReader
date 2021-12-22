#include "CANControllerDetector.h"
#include "WindowsCANController.h"

using namespace can;

std::vector<CANDevice> CANControllerDetector::EnumerateDevices()
{
    return WindowsCANController::EnumerateDevices();
}

std::unique_ptr<CANController> CANControllerDetector::CreateController()
{
    return std::make_unique<WindowsCANController>();
}
