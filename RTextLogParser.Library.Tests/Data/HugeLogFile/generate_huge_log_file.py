# region settings
import datetime
import os
from random import randint, random, choice

file_name = 'HugeLogFile.txt'
log_lines = int(1e6)
scope_max_level = 5
scope_change_rate = 0.01
start_date = datetime.datetime(2020, 5, 6)
log_date_increment_range_ns = (1, 60000000000)  # 1 ns to 1 minute
error_levels = ['ERROR', 'WARNING', 'INFO', 'DEBUG']
scope_level_name = 'SCOPE'
level_length = 7
separator = '|'
messages = ['This is test message', 'Another test message', 'Non voluptate quos non asperiores',
            'Pariatur nesciunt velit est. Molestias et a voluptatem quam est',
            'Ut sunt magni minus quo voluptas voluptas', 'Duis aute irure dolor in reprehenderit in voluptate',
            'velit esse cillum dolore eu fugiat nulla pariatur', 'Ut enim ad minim veniam']


# endregion settings


def format_date(date: datetime) -> str:
    return date.strftime("%Y-%m-%dT%H:%M:%S.%f")


def get_randomized_level_or_scope() -> str:
    if random() <= scope_change_rate:
        return scope_level_name
    else:
        return choice(error_levels)


def get_spaces_for_scope_level(scope_level: int) -> str:
    return 2*scope_level*' '


def generate_file():
    with open(file_name, "w") as f:
        log_current_date = start_date
        scope_level = 0
        for _ in range(log_lines):
            randomized_level = get_randomized_level_or_scope()
            f.write(' '.join([f'[{format_date(log_current_date)}]', randomized_level.ljust(level_length), separator,
                              get_spaces_for_scope_level(scope_level), choice(messages)]) + os.linesep)
            # update variables
            log_current_date = log_current_date + datetime.timedelta(
                microseconds=randint(log_date_increment_range_ns[0], log_date_increment_range_ns[1])/1000)
            if randomized_level == scope_level_name:
                if scope_level == 0:
                    scope_level += 1
                elif scope_level == scope_max_level:
                    scope_level -= 1
                else:
                    scope_level = scope_level + (1 if random() < 0.5 else -1)


if __name__ == '__main__':
    generate_file()
