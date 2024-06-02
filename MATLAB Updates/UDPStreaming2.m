clear;
clc;

% Creating Path to needed class libraries
% include x-IMU MATLAB library
addpath(['C:\Users\mccom\Documents\GitHub\VirtualOR2\MATLAB Updates\ximu_matlab_library']);	
% include quaternion library
addpath(['C:\Users\mccom\Documents\GitHub\VirtualOR2\MATLAB Updates\quaternion_library']);
% Importing Mahony Function
import MahonyAHRS.*

% Initialize Arduino and IMU
UNO = arduino('COM3', 'Uno', 'Libraries', 'I2C');
imu = mpu6050(UNO, 'SampleRate', 200, 'SamplesPerRead', 1);
fuse = imufilter('SampleRate', 200);

% Define threshold for zeroing gyroscope readings (in deg/s)
gyroZeroThreshold = 0.5; % Adjust as needed

% UDP setup
receiverIP = '127.0.0.1';  % Unity host IP
receiverPort = 8000;       % Unity host port
udpClient = udp(receiverIP, receiverPort, 'LocalPort', 0);
fopen(udpClient);

disp('Connected to Unity host.');

% Initialize arrays for displacement calculation
ahrs = MahonyAHRS('SamplePeriod', 1/200, 'Kp', 1);
nyquistFreq = 200 / 2; % Nyquist frequency
cutoff_freq = 0.4 / nyquistFreq; % Normalize cutoff frequency
[b, a] = butter(1, cutoff_freq, 'high');
z_velocity = zeros(max(length(a), length(b)) - 1, 3);
z_position = zeros(max(length(a), length(b)) - 1, 3);

linear_velocity = zeros(1, 3);
linear_position = zeros(1, 3);

startTime = tic;

while true
    try
        data = table2array(imu.read());
        accel = [-data(:, 2), -data(:, 1), data(:, 3)];
        gyro = [data(:, 5), data(:, 4), -data(:, 6)];

        % Fuse accelerometer and gyroscope data to estimate device orientation
        Viz_data = fuse(accel, gyro);

        % Calculate magnitude of angular velocity vector
        gyroMagnitude = sqrt(sum(gyro.^2, 2));

        % Zero gyroscope readings if angular velocity is below threshold
        gyro(gyroMagnitude < gyroZeroThreshold, :) = 0;

        % Implementing AHRS algorithms 
        % Initializing orientation
        ahrs.UpdateIMU(gyro * (pi / 180), accel);
        R = quatern2rotMat(ahrs.Quaternion);

        % Calculate 'tilt-compensated' accelerometer
        tc_accel = R * accel';

        % Calculate linear acceleration in Earth frame (subtracting gravity)
        linear_accel = (tc_accel - [0; 0; 1])' * 9.81;

        % Calculate linear velocity (integrate acceleration)
        linear_velocity = linear_velocity + linear_accel * (1 / 200);

        % Filtering linear velocity for each dimension
        cleaned_linear_velocity = zeros(1, 3);
        for dim = 1:3
            [cleaned_linear_velocity(dim), z_velocity(:, dim)] = filter(b, a, linear_velocity(dim), z_velocity(:, dim));
        end

        % Calculate linear position (integrate velocity)
        linear_position = linear_position + cleaned_linear_velocity * (1 / 200);

        % High-pass filter linear position to remove drift
        cleaned_linear_position = zeros(1, 3);
        for dim = 1:3
            [cleaned_linear_position(dim), z_position(:, dim)] = filter(b, a, linear_position(dim), z_position(:, dim));
        end

        % Displacement in meters
        DisplacementX = cleaned_linear_position(1) / 175;
        DisplacementY = cleaned_linear_position(2) / 175;
        DisplacementZ = cleaned_linear_position(3) / 175;

        % Get quaternion parts
        [q0, q1, q2, q3] = parts(Viz_data);

        % Print orientation and displacement data in one line
        fprintf('Orientation: [%f, %f, %f, %f], Displacement: [%f, %f, %f]\n', ...
            q0, q1, q2, q3, DisplacementX, DisplacementY, DisplacementZ);

        % Send data to Unity via UDP
        dataStr = sprintf('%.6f,%.6f,%.6f,%.6f', q0, q1, q2, q3);
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
