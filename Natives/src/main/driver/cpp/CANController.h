#pragma once

#include <stdint.h>
#include <optional>
#include <vector>
#include <string>
#include "wpi/Twine.h"
#include "FRC_CAN_Reader_Native.h"

class CANController {
public:
    virtual ~CANController() = default;
    virtual std::optional<CANData> getData() = 0;
    virtual void releaseData() = 0;
    virtual void selectDevice(std::string port) = 0;
};
