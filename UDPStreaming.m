clear;
clc;

% Initialize Arduino and IMU
UNO = arduino('COM3', 'Uno', 'Libraries', 'I2C');
imu = mpu6050(UNO, 'SampleRate', 200);
fuse = imufilter('SampleRate', 200);

% Define threshold for zeroing gyroscope readings (in deg/s)
gyroZeroThreshold = 0.5; % Adjust as needed

% UDP setup
receiverIP = '127.0.0.1';  % Unity host IP
receiverPort = 8000;       % Unity host port
udpClient = udp(receiverIP, receiverPort, 'LocalPort', 0);
fopen(udpClient);

disp('Connected to Unity host.');

startTime = tic;
calibrationInterval = 5; 
gyroBias = [0, 0, 0]; 

while true
    try
        data = table2array(imu.read());
        accel = [-data(:, 2), -data(:, 1), data(:, 3)];
        gyro = [data(:, 5), data(:, 4), -data(:, 6)];

        if toc(startTime) >= calibrationInterval
            gyroBias = mean(gyro, 1);
            calibrationStartTime = toc(startTime);
        end
        %gyro = gyro - gyroBias;
        gyroMagnitude = sqrt(sum(gyro.^2, 2));
        gyro(gyroMagnitude < gyroZeroThreshold, :) = 0;
        Viz_data = fuse(accel, gyro);
        q = Viz_data(end);  % Get the latest quaternion. .Quaternion method does not work but the array is already in the correct format
        [q0, q1, q2, q3] = parts(q);
        dataStr = sprintf('%.6f,%.6f,%.6f,%.6f', q0, q1, q2, q3);
        disp(['Data string sent to Unity: ', dataStr]);
        fwrite(udpClient, dataStr);
        pause(0.01);
    catch ME
        % If an error occurs, display the error message and attempt to reconnect
        disp(['Error: ', ME.message]);
        disp('Attempting to reconnect to Unity host...');
        fclose(udpClient);
        fopen(udpClient);
    end
end

% Close UDP connection on exit
fclose(udpClient);
