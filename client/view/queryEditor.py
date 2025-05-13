import random

from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
import re
import random

class QueryEditor(QLineEdit):
    def __init__(self, parent=None):
        super(QueryEditor, self).__init__(parent)
        self.setPlaceholderText("Write query here")
        self.words = ["SELECT", "WHERE", "FROM", "JOIN", "DELETE", "AND"]
        self.table_names = None
        shortcut = QShortcut(Qt.Key_Tab, self)
        shortcut.activated.connect(self.handle_tab)

        self.rows = self.read_insert_data()

    def set_table_names(self, table_names):
        self.table_names = table_names

    def find_previous_space(self, current_text, current_pos):
        i = current_pos - 1
        while i > -1:
            if current_text[i] == " ":
                return current_text[:i+1]
            i -= 1
        return ""

    def handle_tab(self):  # TAB key
        print('Tab')
        current_text = self.text()
        if not current_text:
            self.setText("SELECT")
            return
        current_words = current_text.split(' ')
        for word in self.words:
            if word.startswith(current_words[-1]) and word not in current_words:
                prev = self.find_previous_space(current_text, self.cursorPosition())
                self.setText(prev + word)
                self.setCursorPosition(len(self.text()))  # Move cursor to the end of the autocompleted word
                return
        for word in self.table_names:
            if word.startswith(current_words[-1]):
                prev = self.find_previous_space(current_text, self.cursorPosition())
                self.setText(prev + word)
                self.setCursorPosition(len(self.text()))  # Move cursor to the end of the autocompleted word
                return

    def add_whitespaces(self, text):
        i = 1
        while i < len(text)-1:
            if text[i] in "<>=" and text[i-1] != ' ' and text[i-1] not in '<>=':
                text = text[:i] + " " + text[i:]
            if text[i] in "<>=" and text[i+1] != ' ' and text[i+1] not in '<>=':
                text = text[:i+1] + " " + text[i+1:]
            i += 1
        return text

    def check_signs(self, words):
        sign = False
        for word in words:
            if word in ",." and sign:
                return False
            elif word in ",.":
                sign = True
            else:
                sign = False
            if word.count(',') > 1 or word.count('.') > 1:
                return False
        return True

    def validate_simple_select(self, sql):
        pattern = r"""
            ^\s*
            (?:
                # SELECT pattern
                SELECT\s+
                (?:DISTINCT\s+)?
                (?:[\w*]+(?:\s*,\s*[\w*]+)*|\*)
                \s+FROM\s+
                [\w]+
            |
                # DELETE pattern
                DELETE\s+FROM\s+
                [\w]+
            )                                # Table name
            (?:\s+WHERE\s+                         # WHERE clause
            (?:                                 # Start condition group
                [\w]+\s*                       # Column name
                (?:=|!=|<=?|>=?|<>|LIKE)\s*    # Comparison operators
                (?:'[^']*'|"[^"]*"|[\d\w.]+)   # Value
                (?:\s+AND\s+                   # AND for additional conditions
                    [\w]+\s*                   # Next column
                    (?:=|!=|<=?|>=?|<>|LIKE)\s*
                    (?:'[^']*'|"[^"]*"|[\d\w.]+)
                )*                             # 0 or more additional conditions
            )
            )?
            \s*;?\s*$                               # Optional semicolon
        """
        return bool(re.fullmatch(pattern, sql, re.IGNORECASE | re.VERBOSE))

    def read_insert_data(self):
        values = []
        with open('players.txt', 'r') as file:
            lines = file.readlines()
            for line in lines:
                values.append(line.split('^'))
        return values

    def get_insert_data(self, command): #TODO: parse insert
        # parts = command.split()
        # table = parts[1]
        # cols = parts[2][1:-1].split(',')
        # columns = [col.strip() for col in cols]
        # rs = parts[4]
        # rows = []
        # return None
        table = "testtable"
        columns = ["name", "age", "tel"]
        return (table, columns, self.rows)


    def get_delete_data(self, command):
        main_table = ""
        conditions = []
        parts = command.split()
        if not self.check_signs(parts):
            return -1, "Incorrect Syntax Error"
        try:
            if parts[0] == 'DELETE' and parts[1] == 'FROM':
                i = 2
                main_table = parts[i].strip()
                i += 1
                if i < len(parts) and parts[i] == 'WHERE':
                    i += 1
                    while i < len(parts):
                        if parts[i] == 'AND':
                            i += 1
                        elif conditions:
                            return -1, "Incorrect Syntax Error"
                        col1 = parts[i].strip()
                        op = parts[i + 1].strip()
                        col2 = parts[i + 2].strip()
                        conditions.append({"col1": col1, "col2": col2, "op": op})
                        i += 3
                return 2, (main_table, conditions)
            else:
                return -1, "Incorrect Syntax Error"
        except IndexError:
            return -1, "Incorrect Syntax Error"

    def parse_command(self):
        command = self.text()
        if command.split()[0] == "INSERT":
            return 1, self.get_insert_data(command)
        if not self.validate_simple_select(command):
            return -1, "Incorrect Syntax Error"
        if command[-1] == ';':
            command = command[:-1]
        command = self.add_whitespaces(command)
        if command.split()[0] == "DELETE":
            return self.get_delete_data(command)

        columns = []
        join_tables = []
        conditions = []
        parts = command.split()
        if not self.check_signs(parts):
            return -1, "Incorrect Syntax Error"
        try:
            if parts[0] == 'SELECT':
                i = 1
                while parts[i] != 'FROM':
                    if ',' in parts[i]:
                        parts2 = parts[i].split(',')
                        for part in parts2:
                            if part.strip():
                                columns.append(part.strip())
                    elif parts[i].strip():
                        columns.append(parts[i].strip())
                    i += 1
                i += 1
                main_table = parts[i].strip()
                i += 1
                while i < len(parts) and parts[i] == 'JOIN':
                    join = {}
                    join["table"] = parts[i+1].strip()
                    if parts[i+2].strip() != 'ON':
                        return -1, "Incorrect Sytax Error"
                    if '=' in parts[i+3]:
                        cols = parts[i+3].split('=')
                        join["col1"] = cols[0].strip()
                        join["col2"] = cols[1].strip()
                        join_tables.append(join)
                        i += 4
                    else:
                        join["col1"] = parts[i+3].strip()
                        join["col2"] = parts[i+5].strip()
                        join_tables.append(join)
                        i += 6
                if i < len(parts) and parts[i] == 'WHERE':
                    i += 1
                    while i < len(parts):
                        if parts[i] == 'AND':
                            i += 1
                        elif conditions:
                            return -1, "Incorrect Syntax Error"
                        col1 = parts[i].strip()
                        op = parts[i+1].strip()
                        col2 = parts[i+2].strip()
                        conditions.append({"col1": col1, "col2": col2, "op": op})
                        i += 3
                return 0, (main_table, columns, join_tables, conditions)
            else:
                return -1, "Incorrect Syntax Error"
        except IndexError:
            return -1, "Incorrect Syntax Error"


