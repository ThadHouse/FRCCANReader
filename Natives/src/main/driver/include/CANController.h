#pragma once

#include <stdint.h>
#include <optional>
#include <vector>
#include <string>
#include "FRC_CAN_Reader_Native.h"

class CANController {
public:
    virtual ~CANController() = default;
    virtual CANData getData() = 0;
    virtual wpi::Event& getEventHandle() = 0;
    virtual void selectDevice(std::string_view port) = 0;
};
