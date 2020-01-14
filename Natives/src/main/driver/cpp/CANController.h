#pragma once

#include <stdint.h>
#include <optional>
#include <vector>
#include <string>
#include "wpi/Twine.h"

struct CANData {
  uint64_t timestamp;
  int32_t id;
  uint8_t length;
  uint8_t data[8];
};

class CANController {
public:
    virtual std::optional<CANData> getData() = 0;
    virtual void stop() = 0;
};
