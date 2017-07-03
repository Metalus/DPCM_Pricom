%% Parâmetros conhecidos do sinal
partition = -1:0.0078125:0.9921875;   % Quantização com 8 bits
codebook = -1:0.0078125:1;   
tempoMax = 2;           % Tempo de sinal
Fs = 16000;             % Frequência de amostragem
BitDepth = 16;          % Bits por amostra
BitRate = BitDepth*Fs;  % Bitrate do sinal
t = linspace(0,tempoMax,Fs*tempoMax);

x=audioread('test.wav');
f=x(:,1)';                  %Pegar mono
sound(x,Fs);
Msq = meansqr(f);
Rij = [ Msq  0.825*Msq 0.562*Msq;
      0.825*Msq Msq 0.825*Msq;
      0.562*Msq 0.825*Msq Msq];
R0 = [0.825*Msq; 0.562*Msq; 0.308*Msq];
aij = Rij\R0;

%Função transferência para o preditor
predictor = [0 aij(1) aij(2) aij(3)]; %m_[k] = 0y[0]+a1*m[k-1]+a2*m[k-2]+a3*m[k-3]

%x = awgn(x,30,'measured');

%% Codificar e decodificar usando DPCM.
%Codificar
[a, encodedx] = dpcmenco(x,codebook,partition,predictor);
%Decodificar
decodedx = dpcmdeco(a,codebook,predictor);

%% Análise do DPCM codificado
figure(1);
plot(t, f,t,encodedx);
title('Análise do sinal codificado em DPCM e passado pra 1 byte');
legend('Sinal original em PCM','Sinal codificado em DPCM','Location','NorthOutside');

%% Análise do DPCM decodificado

figure(2);
plot(t,f,t,decodedx)
title('Sinal recuperado com DPCM');
legend('Original signal','Decoded signal','Location','NorthOutside');

distor = sum((f-decodedx).^2)/length(f)
pause(2);
sound(decodedx,Fs);
audiowrite('decoded.wav',[decodedx decodedx],Fs,'BitsPerSample',16);
audiowrite('dpcm.wav',[encodedx encodedx],Fs,'BitsPerSample',8);