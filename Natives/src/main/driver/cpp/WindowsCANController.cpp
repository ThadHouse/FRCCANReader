#ifdef _WIN32
#include "WindowsCANController.h"

#include "candle.h"
#include <wpi/timestamp.h>
#include <cstring>
#include <wpi/ConcurrentQueue.h>

int WindowsCANController::start(string_view_type port, int baud)
{
    candle_list_handle listHandle;
    if (!candle_list_scan(&listHandle))
    {
        return -1;
    }

    uint8_t length = 0;
    if (!candle_list_length(listHandle, &length))
    {
        candle_list_free(listHandle);
        return -1;
    }

    candle_handle handle;
    bool found = false;

    for (uint16_t i = 0; i < length; i++)
    {

        if (!candle_dev_get(listHandle, i, &handle))
        {
            continue;
        }

        auto path = candle_dev_get_path(handle);
        auto pathView = string_view_type{path};
        if (port == pathView)
        {
            found = true;
            break;
        }
        candle_dev_free(handle);
    }

    if (!found)
    {
        return -1;
    }

    if (!candle_dev_open(handle))
    {
        candle_dev_free(handle);
        return -1;
    }

    if (!candle_channel_set_bitrate(handle, 0, baud))
    {
        candle_dev_close(handle);
        candle_dev_free(handle);
        return -1;
    }

    if (!candle_channel_start(handle, 0, CANDLE_MODE_NORMAL))
    {
        candle_dev_close(handle);
        candle_dev_free(handle);
        return -1;
    }

    m_candleHandle = handle;

    m_running = true;
    m_incomingThread = std::thread(&WindowsCANController::readThreadMain, this);

    return 0;
}
void WindowsCANController::stop()
{
    m_running = false;
    m_incomingData.emplace(CANData{});
    m_incomingThread.join();

    if (m_candleHandle)
    {
        candle_channel_stop(m_candleHandle, 0);
        candle_dev_close(m_candleHandle);
        candle_dev_free(m_candleHandle);
    }
}
std::optional<CANData> WindowsCANController::getData()
{
    auto ret = m_incomingData.pop();
    if (!m_running)
        return {};
    return ret;
}

std::vector<std::pair<WindowsCANController::string_type, WindowsCANController::string_type>> WindowsCANController::getDevices()
{
    candle_list_handle listHandle;
    if (!candle_list_scan(&listHandle))
    {
        return {};
    }

    uint8_t length = 0;
    if (!candle_list_length(listHandle, &length))
    {
        candle_list_free(listHandle);
        return {};
    }

    std::vector<std::pair<WindowsCANController::string_type, WindowsCANController::string_type>> paths;

    for (uint16_t i = 0; i < length; i++)
    {
        candle_handle handle;
        if (!candle_dev_get(listHandle, i, &handle))
        {
            continue;
        }

        auto path = candle_dev_get_path(handle);
        auto name = candle_dev_get_name(handle);
        paths.emplace_back(std::make_pair(name, path));
        candle_dev_free(handle);
    }

    candle_list_free(listHandle);

    return paths;
}

void WindowsCANController::readThreadMain()
{
    candle_frame_t frame;

    while (m_running)
    {
        if (!candle_frame_read(m_candleHandle, &frame, 1000))
        {
            continue;
        }

        CANData newData;
        newData.id = candle_frame_id(&frame);

        auto dlc = candle_frame_dlc(&frame);
        auto data = candle_frame_data(&frame);

        int i = 0;
        for (; i < dlc; i++)
        {
            newData.data[i] = data[i];
        }

        for (; i < 8; i++)
        {
            newData.data[i] = 0;
        }

        newData.length = dlc;

        newData.timestamp = wpi::Now();
        
        m_incomingData.emplace(newData);
    }
}
#endif
