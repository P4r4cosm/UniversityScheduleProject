FROM docker.elastic.co/elasticsearch/elasticsearch:8.4.3

USER root  # Временно переключаемся на root
RUN apt-get update --allow-releaseinfo-change && \
    apt-get install -y locales && \
    sed -i '/ru_RU.UTF-8/s/^# //g' /etc/locale.gen && \
    locale-gen ru_RU.UTF-8

ENV LANG ru_RU.UTF-8
ENV LANGUAGE ru_RU:ru
ENV LC_ALL ru_RU.UTF-8

USER elasticsearch  # Возвращаем пользователя обратно
