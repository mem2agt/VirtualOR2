clear;
clc;

% Creating Path to needed class libraries
% include x-IMU MATLAB library
addpath(['C:\Users\Student\OneDrive\Desktop\MATLAB Updates\ximu_matlab_library']);	
% include quaternion library
addpath(['C:\Users\Student\OneDrive\Desktop\MATLAB Updates\quaternion_library']);
% Importing Mahony Function
import MahonyAHRS.*

%% Creating Connection To Arduino Uno
% Create connection to Arduino Uno
UNO = arduino('COM12', 'Uno', 'Libraries', 'I2C');
% Define Sample rate usually 100 hz or 200 hz
imu = mpu6050(UNO, 'SampleRate', 200, 'SamplesPerRead', 1);

%% Time Parameters, Sample Rate & Period
% Define time (in seconds) visualization will run
stopTimer = 5;
% Get the current time
ts = tic;

SampleRate = 200;
SamplePeriod = 1/200;

%% Defining Filter, Thresholding, Initializing Arrays for orientation calc
% Initialize Kalman filter
fuse = imufilter('SampleRate',200);

% Define threshold for zeroing gyroscope readings (in deg/s)
gyroZeroThreshold = 0.5; % Adjust as needed

% Initialize arrays to store raw data
AccData = zeros(stopTimer*SampleRate, 3);
GyroData = zeros(stopTimer*SampleRate, 3);

%% Initializing Arrays for displacement calc
% Calling AHRS function
ahrs = MahonyAHRS('SamplePeriod', SamplePeriod, 'Kp', 1);

% Filter coefficients for high-pass filter
cutoff_freq = (2 * 0.4) / (1 / SamplePeriod);
[b, a] = butter(1, cutoff_freq, 'high');

% State variables for the filter (one for each dimension)
z_velocity = zeros(max(length(a), length(b)) - 1, 3);
z_position = zeros(max(length(a), length(b)) - 1, 3);

% Initialize arrays to store calculated data
tc_accel = zeros(stopTimer*SampleRate, 3);
linear_accel = zeros(stopTimer*SampleRate, 3);
linear_velocity = zeros(stopTimer*SampleRate, 3);
cleaned_linear_velocity = zeros(stopTimer*SampleRate, 3);
linear_position = zeros(stopTimer*SampleRate, 3);
cleaned_linear_position = zeros(stopTimer*SampleRate, 3);

% Initialize separate arrays to store displacement data for each axis
DisplacementX = zeros(stopTimer*SampleRate, 1);
DisplacementY = zeros(stopTimer*SampleRate, 1);
DisplacementZ = zeros(stopTimer*SampleRate, 1);

%% UDP Setup
receiverIP = '127.0.0.1';  % Unity host IP
receiverPort = 8000;       % Unity host port
udpClient = udp(receiverIP, receiverPort, 'LocalPort', 0);
fopen(udpClient);
disp('Connected to Unity host.');

%% Loop
% Creating index for storing data 
i = 1; 

while (toc(ts) < stopTimer)
    % Get the current elapsed time
    elapsedTime = toc(ts);
    disp(['Current time: ', num2str(elapsedTime), ' seconds']);

    % Read raw data from IMU 
    % Data will be provided as a time table therefore, must use the 
    % table2array function 
    data = table2array(imu.read());

    % Assign the raw data measurements to accel and gyro 
    accel = [-data(:,2), -data(:,1), data(:,3)];
    gyro = [data(:,5), data(:,4), -data(:,6)];

    % Fuse accelerometer and gyroscope data to estimate device orientation
    Viz_data = fuse(accel, gyro);

    % Calculate magnitude of angular velocity vector
    gyroMagnitude = sqrt(sum(gyro.^2, 2));

    % Zero gyroscope readings if angular velocity is below threshold
    gyro(gyroMagnitude < gyroZeroThreshold, :) = 0;

    % Storing Data 
    AccData(i,:) = accel;
    GyroData(i,:) = gyro;

    % Implementing AHRS algorithms 
    % Initializing orientation
    R = zeros(3,3);
    ahrs.UpdateIMU(gyro*(pi/180),AccData(i,:));
    R = quatern2rotMat(ahrs.Quaternion);

    % Calculate 'tilt-compensated' accelerometer
    tc_accel(i,:) = R * AccData(i,:)';

    % Calculate linear acceleration in Earth frame (subtracting gravity)
    linear_accel(i,:) = (tc_accel(i,:) - [0, 0, 1]) * 9.81;

    % Calculate linear velocity (integrate acceleration)
    if i >= 2
        linear_velocity(i,:) = linear_velocity(i-1,:) + linear_accel(i,:) * SamplePeriod;
    end

    % Filtering linear velocity for each dimension
    for dim = 1:3
        [cleaned_linear_velocity(i, dim), z_velocity(:, dim)] = filter(b, a, linear_velocity(i, dim), z_velocity(:, dim));
    end

    % Calculate linear position (integrate velocity)
    if i >= 2
        linear_position(i,:) = linear_position(i-1,:) + cleaned_linear_velocity(i,:) * SamplePeriod;
    end

    % High-pass filter linear position to remove drift
    for dim = 1:3
        [cleaned_linear_position(i, dim), z_position(:, dim)] = filter(b, a, linear_position(i, dim), z_position(:, dim));
    end

    % Store the displacement data in separate arrays
    % Important to note that units of measure is: meters 
    DisplacementX(i) = linear_position(i, 1)/175;
    DisplacementY(i) = linear_position(i, 2)/175;
    DisplacementZ(i) = linear_position(i, 3)/175;

    % Convert data to string
    dataStr = sprintf('%.6f,%.6f,%.6f,%.6f', linear_position(i,1), linear_position(i,2), linear_position(i,3), gyro(1), gyro(2), gyro(3));

    % Send data over UDP
    fwrite(udpClient, dataStr);

    i = i + 1; 
end

% Close UDP connection
fclose(udpClient);
