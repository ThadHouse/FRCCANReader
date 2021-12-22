#pragma once

#include <string>
#include <vector>
#include "FRC_CAN_Reader_Native.h"
#include "CANController.h"

namespace can
{
  struct CANDevice
  {
    std::string name;
    std::string deviceId;
  };

  class CANControllerDetector
  {
  public:
    static std::vector<CANDevice> EnumerateDevices();
    static std::unique_ptr<CANController> CreateController();
  };
}
