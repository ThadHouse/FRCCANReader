#pragma once

#include <stdint.h>
#include <optional>
#include <vector>
#include "wpi/mutex.h"
#include <string>
#include "FRC_CAN_Reader_Native.h"

class CANController {
public:
    virtual ~CANController() = default;
    virtual void SelectDevice(std::string_view port) = 0;

    std::vector<CANData> GetData() {
        std::scoped_lock lock{m_dataMutex};
        m_dataEvent.Reset();
        if (m_data.empty()) {
            return {};
        }
        std::vector<CANData> retData;
        m_data.swap(retData);
        return retData;
    }
    const wpi::Event& GetEventHandle() const { return m_dataEvent; }
    wpi::Event& GetEventHandle() { return m_dataEvent; }

protected:
    void PushData(CANData&& data) {
        std::scoped_lock lock{m_dataMutex};
        m_data.emplace_back(std::forward<CANData>(data));
        m_dataEvent.Set();
    }

    wpi::mutex m_dataMutex;
    std::vector<CANData> m_data;
    wpi::Event m_dataEvent;
};
